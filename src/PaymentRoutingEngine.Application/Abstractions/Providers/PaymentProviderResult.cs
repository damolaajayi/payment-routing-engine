using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Providers
{
    public sealed record PaymentProviderResult(
    bool IsSuccess,
    string? ProviderReference,
    string? FailureReason);
}
