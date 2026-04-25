using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Providers
{
    public interface IPaymentProviderResolver
    {
        IPaymentProviderClient Resolve(PaymentProvider provider);
    }
}
