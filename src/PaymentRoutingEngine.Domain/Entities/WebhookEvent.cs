using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Domain.Entities
{
    public sealed class WebhookEvent
    {
        private WebhookEvent()
        {
        }

        public Guid Id { get; private set; }
        public PaymentProvider Provider { get; private set; }
        public string? ExternalEventId { get; private set; }
        public string? Signature { get; private set; }
        public string? EventType { get; private set; }
        public string Payload { get; private set; } = default!;
        public short ProcessingStatus { get; private set; }
        public DateTime ReceivedAtUtc { get; private set; }
        public DateTime? ProcessedAtUtc { get; private set; }
        public Guid? PaymentTransactionId { get; private set; }
        public string? ErrorMessage { get; private set; }
    }
}
