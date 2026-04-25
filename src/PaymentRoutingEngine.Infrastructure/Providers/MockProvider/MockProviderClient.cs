using PaymentRoutingEngine.Application.Abstractions.Providers;
using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PaymentRoutingEngine.Infrastructure.Providers.MockProvider
{
    public sealed class MockProviderClient : IPaymentProviderClient
    {
        public PaymentProvider Provider => PaymentProvider.MockProvider;

        public Task<ProviderPaymentResponse> ProcessPaymentAsync(
            ProviderPaymentRequest request,
            CancellationToken cancellationToken = default)
        {
            var random = Random.Shared.Next(0, 100);

            if (random < 70)
            {
                return Task.FromResult(new ProviderPaymentResponse
                {
                    IsSuccessful = true,
                    ProviderReference = $"MOCK-{Guid.NewGuid():N}",
                    ProviderStatusCode = "00",
                    FailureCategory = FailureCategory.None,
                    RawResponsePayload = JsonSerializer.Serialize(new
                    {
                        reference = request.Reference,
                        status = "success",
                        providerStatusCode = "00"
                    })
                });
            }

            return Task.FromResult(new ProviderPaymentResponse
            {
                IsSuccessful = false,
                ProviderReference = null,
                ProviderStatusCode = "96",
                FailureCategory = FailureCategory.ProviderTransient,
                FailureReason = "Mock provider transient failure.",
                RawResponsePayload = JsonSerializer.Serialize(new
                {
                    reference = request.Reference,
                    status = "failed",
                    providerStatusCode = "96"
                })
            });
        }
    }
}
