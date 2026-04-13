using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Domain.Enums
{
    public enum AttemptStatus : short
    {
        Pending = 1,
        Processing = 2,
        Succeeded = 3,
        Failed = 4
    }
}
