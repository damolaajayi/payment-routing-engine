using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Persistence.Configurations
{
    public sealed class TransactionStatusHistoryConfiguration : IEntityTypeConfiguration<TransactionStatusHistoryEntry>
    {
        public void Configure(EntityTypeBuilder<TransactionStatusHistoryEntry> builder)
        {
            builder.ToTable("transaction_status_history");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.PaymentTransactionId)
                .HasColumnName("payment_transaction_id")
                .IsRequired();

            builder.Property(x => x.PreviousStatus)
                .HasColumnName("previous_status")
                .HasConversion<short?>();

            builder.Property(x => x.NewStatus)
                .HasColumnName("new_status")
                .HasConversion<short>()
                .IsRequired();

            builder.Property(x => x.Reason)
                .HasColumnName("reason")
                .HasMaxLength(500);

            builder.Property(x => x.Source)
                .HasColumnName("source")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ChangedAtUtc)
                .HasColumnName("changed_at_utc")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb");

            builder.HasIndex(x => x.PaymentTransactionId);
            builder.HasIndex(x => x.ChangedAtUtc);
        }
    }
}
