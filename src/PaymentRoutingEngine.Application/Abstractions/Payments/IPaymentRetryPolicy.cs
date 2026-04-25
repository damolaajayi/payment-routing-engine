using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Payments
{
    public interface IPaymentRetryPolicy
    {
        bool IsRetryable(FailureCategory failureCategory);
    }
}
