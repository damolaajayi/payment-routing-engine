using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Providers
{
    public interface IPaymentProviderClient
    {
        PaymentProvider Provider { get; }

        Task<ProviderPaymentResponse> ProcessPaymentAsync(
            ProviderPaymentRequest request,
            CancellationToken cancellationToken = default);
    }
}
