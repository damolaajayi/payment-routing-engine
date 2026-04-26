using Microsoft.Extensions.Options;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Application.Payments.ProcessPayment;
using PaymentRoutingEngine.Infrastructure.Messaging;
using PaymentRoutingEngine.Infrastructure.Messaging.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PaymentRoutingEngine.Workers.Consumers
{
    public sealed class ProcessPaymentConsumer : BackgroundService
    {
        private const string RetryCountHeader = "x-retry-count";
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMqConnectionProvider _connectionProvider;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<ProcessPaymentConsumer> _logger;

        private IChannel? _channel;

        public ProcessPaymentConsumer(
            IServiceProvider serviceProvider,
            RabbitMqConnectionProvider connectionProvider,
            IOptions<RabbitMqOptions> options,
            ILogger<ProcessPaymentConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _connectionProvider = connectionProvider;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = await _connectionProvider.GetConnectionAsync(stoppingToken);

            _channel = await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);


            await DeclareQueuesAsync(_channel, stoppingToken);

            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 5,
                global: false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                await HandleMessageAsync(eventArgs, stoppingToken);
            };

            await _channel.BasicConsumeAsync(
                queue: _options.ProcessPaymentQueue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation(
            "Process payment consumer started. Queue: {Queue}, DLQ: {DeadLetterQueue}",
            _options.ProcessPaymentQueue,
            _options.ProcessPaymentDeadLetterQueue);
        }

        private async Task HandleMessageAsync(
            BasicDeliverEventArgs eventArgs,
            CancellationToken cancellationToken)
        {

            var body = eventArgs.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            ProcessPaymentMessage? message = null;
            try
            {
                message = JsonSerializer.Deserialize<ProcessPaymentMessage>(json);

                if (message is null)
                {
                    await _channel!.BasicRejectAsync(
                        deliveryTag: eventArgs.DeliveryTag,
                        requeue: false,
                        cancellationToken: cancellationToken);

                    return;
                }

                using var scope = _serviceProvider.CreateScope();

                var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();

                await dispatcher.Send(
                    new ProcessPaymentCommand(message.TransactionId),
                    cancellationToken);

                await _channel!.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: cancellationToken);

                _logger.LogInformation(
                "Payment processing message acknowledged. TransactionId: {TransactionId}, DeliveryTag: {DeliveryTag}",
                message.TransactionId,
                eventArgs.DeliveryTag);
            }
            catch (Exception exception)
            {
                var retryCount = GetRetryCount(eventArgs);

                _logger.LogError(
                    exception,
                    "Failed to process payment message. TransactionId: {TransactionId}, RetryCount: {RetryCount}, DeliveryTag: {DeliveryTag}",
                    message?.TransactionId,
                    retryCount,
                    eventArgs.DeliveryTag);

                if (retryCount >= _options.MaxProcessingRetries)
                {
                    await MoveToDeadLetterQueueAsync(
                        eventArgs,
                        body,
                        reason: exception.Message,
                        cancellationToken);

                    return;
                }

                await RetryMessageAsync(
                    eventArgs,
                    body,
                    retryCount + 1,
                    cancellationToken);
            }
        }

        private async Task RetryMessageAsync(
        BasicDeliverEventArgs eventArgs,
        byte[] body,
        int nextRetryCount,
        CancellationToken cancellationToken)
        {
            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = eventArgs.BasicProperties?.MessageId ?? Guid.NewGuid().ToString("N"),
                CorrelationId = eventArgs.BasicProperties?.CorrelationId,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Headers = new Dictionary<string, object?>
                {
                    [RetryCountHeader] = nextRetryCount
                }
            };

            await _channel!.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.ProcessPaymentQueue,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            await _channel.BasicAckAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);

            _logger.LogWarning(
                "Payment processing message requeued. RetryCount: {RetryCount}, DeliveryTag: {DeliveryTag}",
                nextRetryCount,
                eventArgs.DeliveryTag);
        }

        private async Task MoveToDeadLetterQueueAsync(
        BasicDeliverEventArgs eventArgs,
        byte[] body,
        string reason,
        CancellationToken cancellationToken)
        {
            var retryCount = GetRetryCount(eventArgs);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = eventArgs.BasicProperties?.MessageId ?? Guid.NewGuid().ToString("N"),
                CorrelationId = eventArgs.BasicProperties?.CorrelationId,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Headers = new Dictionary<string, object?>
                {
                    [RetryCountHeader] = retryCount,
                    ["x-dead-letter-reason"] = reason,
                    ["x-dead-lettered-at"] = DateTimeOffset.UtcNow.ToString("O")
                }
            };

            await _channel!.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.ProcessPaymentDeadLetterQueue,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            await _channel.BasicAckAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);

            _logger.LogError(
                "Payment processing message moved to DLQ. RetryCount: {RetryCount}, Reason: {Reason}, DeliveryTag: {DeliveryTag}",
                retryCount,
                reason,
                eventArgs.DeliveryTag);
        }

        private static int GetRetryCount(BasicDeliverEventArgs eventArgs)
        {
            if (eventArgs.BasicProperties?.Headers is null)
                return 0;

            if (!eventArgs.BasicProperties.Headers.TryGetValue(RetryCountHeader, out var value))
                return 0;

            return value switch
            {
                int intValue => intValue,
                long longValue => (int)longValue,
                byte byteValue => byteValue,
                short shortValue => shortValue,
                byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out var parsed) => parsed,
                _ => 0
            };
        }

        private async Task DeclareQueuesAsync(
        IChannel channel,
        CancellationToken cancellationToken)
        {
            await channel.QueueDeclareAsync(
                queue: _options.ProcessPaymentQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: _options.ProcessPaymentDeadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);
        }


        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
