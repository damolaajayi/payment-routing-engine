using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Persistence.Configurations
{
    public sealed class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
    {
        public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
        {
            builder.ToTable("payment_attempts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.PaymentTransactionId)
                .HasColumnName("payment_transaction_id")
                .IsRequired();

            builder.Property(x => x.AttemptNumber)
                .HasColumnName("attempt_number")
                .IsRequired();

            builder.Property(x => x.Provider)
                .HasColumnName("provider")
                .HasConversion<short>()
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<short>()
                .IsRequired();

            builder.Property(x => x.ProviderReference)
                .HasColumnName("provider_reference")
                .HasMaxLength(150);

            builder.Property(x => x.ProviderStatusCode)
                .HasColumnName("provider_status_code")
                .HasMaxLength(50);

            builder.Property(x => x.FailureCategory)
                .HasColumnName("failure_category")
                .HasConversion<short?>();

            builder.Property(x => x.FailureReason)
                .HasColumnName("failure_reason")
                .HasMaxLength(500);

            builder.Property(x => x.IsRetry)
                .HasColumnName("is_retry")
                .IsRequired();

            builder.Property(x => x.IsFailover)
                .HasColumnName("is_failover")
                .IsRequired();

            builder.Property(x => x.RequestPayload)
                .HasColumnName("request_payload")
                .HasColumnType("jsonb");

            builder.Property(x => x.ResponsePayload)
                .HasColumnName("response_payload")
                .HasColumnType("jsonb");

            builder.Property(x => x.StartedAtUtc)
                .HasColumnName("started_at_utc")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.CompletedAtUtc)
                .HasColumnName("completed_at_utc")
                .HasColumnType("timestamptz");

            builder.Property(x => x.NextRetryAtUtc)
                .HasColumnName("next_retry_at_utc")
                .HasColumnType("timestamptz");

            builder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.HasIndex(x => new { x.PaymentTransactionId, x.AttemptNumber })
                .IsUnique();

            builder.HasIndex(x => x.Provider);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.NextRetryAtUtc);
        }
    }
}
