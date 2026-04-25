using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Payments.ProcessPayment
{
    public sealed record ProcessPaymentResult(
    Guid TransactionId,
    string Reference,
    TransactionStatus Status,
    string? FailureReason,
    string? ProviderReference);
}
