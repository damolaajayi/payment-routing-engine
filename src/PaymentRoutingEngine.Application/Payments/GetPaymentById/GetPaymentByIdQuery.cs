using PaymentRoutingEngine.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Payments.GetPaymentById
{
    public sealed record GetPaymentByIdQuery(Guid TransactionId)
    : IQuery<GetPaymentByIdResult>;
}
