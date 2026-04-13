using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Persistence
{
    public interface IPaymentAttemptRepository
    {
        Task AddAsync(PaymentAttempt attempt, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentAttempt>> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
    }
}
