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

            await _channel.QueueDeclareAsync(
                queue: _options.ProcessPaymentQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

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
                "Process payment consumer started. Queue: {Queue}",
                _options.ProcessPaymentQueue);
        }

        private async Task HandleMessageAsync(
            BasicDeliverEventArgs eventArgs,
            CancellationToken cancellationToken)
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var message = JsonSerializer.Deserialize<ProcessPaymentMessage>(json);

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
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to process payment message. DeliveryTag: {DeliveryTag}",
                    eventArgs.DeliveryTag);

                await _channel!.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: cancellationToken);
            }
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
