using PaymentRoutingEngine.Domain.Entities;
using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Providers
{
    public interface IPaymentProviderRoutingService
    {
        PaymentProvider GetPrimaryProvider(PaymentTransaction transaction);

        IReadOnlyList<PaymentProvider> GetFailoverProviders(
            PaymentTransaction transaction,
            PaymentProvider failedProvider);
    }
}
