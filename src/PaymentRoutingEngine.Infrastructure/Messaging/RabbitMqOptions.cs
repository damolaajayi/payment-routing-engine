using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Messaging
{
    public sealed class RabbitMqOptions
    {
        public string HostName { get; init; } = "localhost";
        public int Port { get; init; } = 5672;
        public string UserName { get; init; } = "guest";
        public string Password { get; init; } = "guest";
        public string VirtualHost { get; init; } = "/";
        public string ProcessPaymentQueue { get; init; } = "process-payment-queue";

        public string ProcessPaymentDeadLetterQueue { get; init; } = "process-payment-dlq";

        public int MaxProcessingRetries { get; init; } = 3;
    }
}
