using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Persistence.Configurations
{
    public sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("payment_transactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Reference)
                .HasColumnName("reference")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.IdempotencyKey)
                .HasColumnName("idempotency_key")
                .HasMaxLength(150);

            builder.Property(x => x.ClientReference)
                .HasColumnName("client_reference")
                .HasMaxLength(150);

            builder.Property(x => x.CustomerId)
                .HasColumnName("customer_id")
                .HasMaxLength(100);

            builder.Property(x => x.AmountMinor)
                .HasColumnName("amount_minor")
                .IsRequired();

            builder.Property(x => x.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<short>()
                .IsRequired();

            builder.Property(x => x.SelectedProvider)
                .HasColumnName("selected_provider")
                .HasConversion<short?>();

            builder.Property(x => x.RoutingDecisionType)
                .HasColumnName("routing_decision_type")
                .HasConversion<short?>();

            builder.Property(x => x.FailureCategory)
                .HasColumnName("failure_category")
                .HasConversion<short?>();

            builder.Property(x => x.FailureReason)
                .HasColumnName("failure_reason")
                .HasMaxLength(500);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.ProcessedAtUtc)
                .HasColumnName("processed_at_utc")
                .HasColumnType("timestamptz");

            builder.Property(x => x.CompletedAtUtc)
                .HasColumnName("completed_at_utc")
                .HasColumnType("timestamptz");

            builder.Property(x => x.Version)
                .HasColumnName("version")
                .IsConcurrencyToken()
                .IsRequired();

            builder.HasIndex(x => x.Reference)
                .IsUnique();

            builder.HasIndex(x => x.IdempotencyKey)
                .IsUnique();

            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.CreatedAtUtc);

            builder.HasMany(x => x.Attempts)
                .WithOne()
                .HasForeignKey(x => x.PaymentTransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.StatusHistory)
                .WithOne()
                .HasForeignKey(x => x.PaymentTransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Attempts)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(x => x.StatusHistory)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
