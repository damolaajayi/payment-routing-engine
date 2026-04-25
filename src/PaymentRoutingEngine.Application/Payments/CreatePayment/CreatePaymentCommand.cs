using PaymentRoutingEngine.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Payments.CreatePayment
{
    public sealed record CreatePaymentCommand(
    string? IdempotencyKey,
    string? ClientReference,
    string? CustomerId,
    long AmountMinor,
    string Currency,
    string? Description) : ICommand<CreatePaymentResult>;
}
