using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Persistence.Configurations
{
    public sealed class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
    {
        public void Configure(EntityTypeBuilder<WebhookEvent> builder)
        {
            builder.ToTable("webhook_events");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Provider)
                .HasColumnName("provider")
                .HasConversion<short>()
                .IsRequired();

            builder.Property(x => x.ExternalEventId)
                .HasColumnName("external_event_id")
                .HasMaxLength(150);

            builder.Property(x => x.Signature)
                .HasColumnName("signature")
                .HasMaxLength(500);

            builder.Property(x => x.EventType)
                .HasColumnName("event_type")
                .HasMaxLength(100);

            builder.Property(x => x.Payload)
                .HasColumnName("payload")
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(x => x.ProcessingStatus)
                .HasColumnName("processing_status")
                .HasConversion<short>()
                .IsRequired();

            builder.Property(x => x.ReceivedAtUtc)
                .HasColumnName("received_at_utc")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.ProcessedAtUtc)
                .HasColumnName("processed_at_utc")
                .HasColumnType("timestamptz");

            builder.Property(x => x.PaymentTransactionId)
                .HasColumnName("payment_transaction_id");

            builder.Property(x => x.ErrorMessage)
                .HasColumnName("error_message")
                .HasMaxLength(1000);

            builder.HasIndex(x => x.Provider);
            builder.HasIndex(x => x.ExternalEventId);
            builder.HasIndex(x => x.ProcessingStatus);
        }
    }
}
