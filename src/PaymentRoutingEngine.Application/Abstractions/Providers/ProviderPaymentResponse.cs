using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Providers
{
    public sealed class ProviderPaymentResponse
    {
        public bool IsSuccessful { get; init; }
        public string? ProviderReference { get; init; }
        public string? ProviderStatusCode { get; init; }
        public FailureCategory? FailureCategory { get; init; }
        public string? FailureReason { get; init; }
        public string? RawResponsePayload { get; init; }
    }
}
