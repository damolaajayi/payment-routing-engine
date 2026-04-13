using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Domain.Enums
{
    public enum TransactionStatus : short
    {
        Pending = 1,
        Processing = 2,
        Succeeded = 3,
        Failed = 4,
        Retrying = 5,
        RequiresManualReview = 6,
        Cancelled = 7
    }
}
