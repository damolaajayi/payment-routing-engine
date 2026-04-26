using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Messaging
{
    public interface IPaymentProcessingQueue
    {
        Task EnqueueAsync(Guid transactionId, CancellationToken cancellationToken = default);
    }
}
