using PaymentRoutingEngine.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Payments.ProcessPayment
{
    public sealed record ProcessPaymentCommand(Guid TransactionId)
    : ICommand<ProcessPaymentResult>;
}
