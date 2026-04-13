using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Persistence
{
    public interface IPaymentTransactionRepository
    {
        Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default);
        Task<PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PaymentTransaction?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
        Task<PaymentTransaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    }
}
