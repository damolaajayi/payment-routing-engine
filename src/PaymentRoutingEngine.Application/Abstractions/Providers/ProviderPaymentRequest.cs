using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Providers
{
    public sealed class ProviderPaymentRequest
    {
        public required string Reference { get; init; }
        public required long AmountMinor { get; init; }
        public required string Currency { get; init; }
        public string? CustomerId { get; init; }
        public string? Description { get; init; }
    }
}
