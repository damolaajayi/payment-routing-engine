using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Domain.Enums
{
    public enum PaymentProvider : short
    {
        Unknown = 0,
        MockProvider = 1,
        Paystack = 2,
        Flutterwave = 3
    }
}
