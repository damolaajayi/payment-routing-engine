using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Payments.CreatePayment
{
    public sealed record CreatePaymentResult(
    Guid TransactionId,
    string Reference,
    string? IdempotencyKey,
    long AmountMinor,
    string Currency,
    TransactionStatus Status,
    DateTime CreatedAtUtc);
}
