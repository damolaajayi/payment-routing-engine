using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Domain.Enums
{
    public enum RoutingDecisionType : short
    {
        None = 0,
        Primary = 1,
        Retry = 2,
        Failover = 3,
        ManualOverride = 4
    }
}
