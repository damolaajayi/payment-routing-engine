using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Domain.Entities
{
    public sealed class OutboxMessage
    {
        private OutboxMessage()
        {
        }

        public Guid Id { get; private set; }
        public string Type { get; private set; } = default!;
        public Guid AggregateId { get; private set; }
        public string Payload { get; private set; } = default!;
        public DateTime OccurredAtUtc { get; private set; }
        public DateTime? ProcessedAtUtc { get; private set; }
        public string? Error { get; private set; }
        public int RetryCount { get; private set; }
    }
}
