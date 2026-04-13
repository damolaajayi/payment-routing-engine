using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Domain.Enums
{
    public enum FailureCategory : short
    {
        None = 0,
        Validation = 1,
        ProviderTransient = 2,
        ProviderPermanent = 3,
        Timeout = 4,
        Network = 5,
        Duplicate = 6,
        Unknown = 7
    }
}
