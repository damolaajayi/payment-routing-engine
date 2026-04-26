using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Infrastructure.Messaging.Messages;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PaymentRoutingEngine.Infrastructure.Messaging
{
    public sealed class RabbitMqPaymentProcessingQueue : IPaymentProcessingQueue
    {
        private const string RetryCountHeader = "x-retry-count";

        private readonly RabbitMqConnectionProvider _connectionProvider;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqPaymentProcessingQueue> _logger;

        public RabbitMqPaymentProcessingQueue(
            RabbitMqConnectionProvider connectionProvider,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqPaymentProcessingQueue> logger)
        {
            _connectionProvider = connectionProvider;
            _options = options.Value;
            _logger = logger;
        }

        public async Task EnqueueAsync(
            Guid transactionId,
            CancellationToken cancellationToken = default)
        {
            var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

            await using var channel = await connection.CreateChannelAsync(
                cancellationToken: cancellationToken); 
            
            await DeclareQueuesAsync(channel, cancellationToken);


            var message = new ProcessPaymentMessage(transactionId);
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var messageId = Guid.NewGuid().ToString("N");

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString("N"),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Headers = new Dictionary<string, object?>
                {
                    [RetryCountHeader] = 0
                }
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.ProcessPaymentQueue,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
            "Published payment processing message. TransactionId: {TransactionId}, Queue: {Queue}, MessageId: {MessageId}",
            transactionId,
            _options.ProcessPaymentQueue,
            messageId);
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
    }
}
