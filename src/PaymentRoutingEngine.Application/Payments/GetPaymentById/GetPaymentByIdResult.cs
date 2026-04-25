using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Payments.GetPaymentById
{
    public sealed record GetPaymentByIdResult(
    Guid TransactionId,
    string Reference,
    string? IdempotencyKey,
    string? ClientReference,
    string? CustomerId,
    long AmountMinor,
    string Currency,
    TransactionStatus Status,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
}
