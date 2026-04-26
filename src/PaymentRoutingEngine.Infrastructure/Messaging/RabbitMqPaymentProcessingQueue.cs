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
        private readonly RabbitMqConnectionProvider _connectionProvider;
        private readonly RabbitMqOptions _options;

        public RabbitMqPaymentProcessingQueue(
            RabbitMqConnectionProvider connectionProvider,
            IOptions<RabbitMqOptions> options)
        {
            _connectionProvider = connectionProvider;
            _options = options.Value;
        }

        public async Task EnqueueAsync(
            Guid transactionId,
            CancellationToken cancellationToken = default)
        {
            var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

            await using var channel = await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: _options.ProcessPaymentQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var message = new ProcessPaymentMessage(transactionId);
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString("N"),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.ProcessPaymentQueue,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
