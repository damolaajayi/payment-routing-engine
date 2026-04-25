using PaymentRoutingEngine.Application.Abstractions.Providers;
using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Providers
{
    public sealed class PaymentProviderResolver : IPaymentProviderResolver
    {
        private readonly Dictionary<PaymentProvider, IPaymentProviderClient> _providers;

        public PaymentProviderResolver(IEnumerable<IPaymentProviderClient> providerClients)
        {
            _providers = providerClients.ToDictionary(p => p.Provider, p => p);
        }

        public IPaymentProviderClient Resolve(PaymentProvider provider)
        {
            if (_providers.TryGetValue(provider, out var client))
                return client;

            throw new InvalidOperationException($"No provider found for {provider}");
        }
    }
}
