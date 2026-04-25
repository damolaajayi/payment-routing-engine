using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Payments
{
    public sealed class PaymentRetryOptions
    {
        public int MaxRetryAttemptsPerProvider { get; init; } = 2;
    }
}
