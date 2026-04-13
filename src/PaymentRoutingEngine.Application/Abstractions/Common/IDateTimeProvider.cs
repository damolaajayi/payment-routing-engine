using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Common
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
