using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Messaging.Messages
{
    public sealed record ProcessPaymentMessage(Guid TransactionId);

}
