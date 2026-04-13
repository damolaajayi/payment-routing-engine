using PaymentRoutingEngine.Application.Abstractions.Providers;
using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Providers.MockProvider
{
    public sealed class MockProviderClient : IPaymentProviderClient
    {
        public PaymentProvider Provider => PaymentProvider.MockProvider;

        public Task<ProviderPaymentResponse> ProcessPaymentAsync(
            ProviderPaymentRequest request,
            CancellationToken cancellationToken = default)
        {
            var isSuccessful = request.AmountMinor <= 500_000;

            return Task.FromResult(new ProviderPaymentResponse
            {
                IsSuccessful = isSuccessful,
                ProviderReference = Guid.NewGuid().ToString("N"),
                ProviderStatusCode = isSuccessful ? "00" : "96",
                FailureCategory = isSuccessful ? null : FailureCategory.ProviderTransient,
                FailureReason = isSuccessful ? null : "Simulated transient provider failure.",
                RawResponsePayload = $$"""
            {
              "reference": "{{request.Reference}}",
              "status": "{{(isSuccessful ? "success" : "failed")}}",
              "providerStatusCode": "{{(isSuccessful ? "00" : "96")}}"
            }
            """
            });
        }
    }
}
