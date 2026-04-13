using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Domain.Entities
{
    public sealed class ProviderRoutingRule
    {
        private ProviderRoutingRule()
        {
        }

        public Guid Id { get; private set; }
        public PaymentProvider Provider { get; private set; }
        public int Priority { get; private set; }
        public string? Currency { get; private set; }
        public long? MinAmountMinor { get; private set; }
        public long? MaxAmountMinor { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }
    }
}
