using Microsoft.EntityFrameworkCore;
using PaymentRoutingEngine.Application.Abstractions.Persistence;
using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Persistence.Repositories
{
    public sealed class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly PaymentRoutingEngineDbContext _dbContext;

        public PaymentTransactionRepository(PaymentRoutingEngineDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
        {
            await _dbContext.PaymentTransactions.AddAsync(transaction, cancellationToken);
        }

        public async Task<PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.PaymentTransactions
                .Include(x => x.Attempts)
                .Include(x => x.StatusHistory)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<PaymentTransaction?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
        {
            return await _dbContext.PaymentTransactions
                .Include(x => x.Attempts)
                .Include(x => x.StatusHistory)
                .FirstOrDefaultAsync(x => x.Reference == reference, cancellationToken);
        }

        public async Task<PaymentTransaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        {
            return await _dbContext.PaymentTransactions
                .Include(x => x.Attempts)
                .Include(x => x.StatusHistory)
                .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
        }
    }
}
