using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Payments
{
    public sealed class PaymentRetryPolicy : IPaymentRetryPolicy
    {
        public bool IsRetryable(FailureCategory failureCategory)
        {
            return failureCategory is
                FailureCategory.ProviderTransient or
                FailureCategory.Timeout or
                FailureCategory.Network or
                FailureCategory.Unknown;
        }
    }
}
