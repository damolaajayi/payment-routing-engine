using PaymentRoutingEngine.Application.Abstractions.Providers;
using PaymentRoutingEngine.Domain.Entities;
using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Providers
{
    public sealed class PaymentProviderRoutingService : IPaymentProviderRoutingService
    {
        public PaymentProvider GetPrimaryProvider(PaymentTransaction transaction)
        {
            if (transaction.Currency.Equals("NGN", StringComparison.OrdinalIgnoreCase))
                return PaymentProvider.Paystack;

            return PaymentProvider.MockProvider;
        }

        public IReadOnlyList<PaymentProvider> GetFailoverProviders(
            PaymentTransaction transaction,
            PaymentProvider failedProvider)
        {
            var providers = new List<PaymentProvider>
        {
            PaymentProvider.MockProvider,
            PaymentProvider.Paystack
            
        };

            return providers
                .Where(x => x != failedProvider && x != PaymentProvider.Unknown)
                .ToList();
        }
    }
}
