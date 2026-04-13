using Microsoft.EntityFrameworkCore;
using PaymentRoutingEngine.Application.Abstractions.Persistence;
using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Persistence.Repositories
{
    public sealed class PaymentAttemptRepository : IPaymentAttemptRepository
    {
        private readonly PaymentRoutingEngineDbContext _dbContext;

        public PaymentAttemptRepository(PaymentRoutingEngineDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(PaymentAttempt attempt, CancellationToken cancellationToken = default)
        {
            await _dbContext.PaymentAttempts.AddAsync(attempt, cancellationToken);
        }

        public async Task<IReadOnlyList<PaymentAttempt>> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.PaymentAttempts
                .Where(x => x.PaymentTransactionId == transactionId)
                .OrderBy(x => x.AttemptNumber)
                .ToListAsync(cancellationToken);
        }
    }
}
