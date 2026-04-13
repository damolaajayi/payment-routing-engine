using Microsoft.EntityFrameworkCore;
using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Persistence
{
    public sealed class PaymentRoutingEngineDbContext : DbContext
    {
        public PaymentRoutingEngineDbContext(DbContextOptions<PaymentRoutingEngineDbContext> options)
            : base(options)
        {
        }

        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
        public DbSet<TransactionStatusHistoryEntry> TransactionStatusHistoryEntries => Set<TransactionStatusHistoryEntry>();
        public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<ProviderRoutingRule> ProviderRoutingRules => Set<ProviderRoutingRule>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentRoutingEngineDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
