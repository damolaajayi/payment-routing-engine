using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Persistence.Configurations
{
    public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.AggregateId)
                .HasColumnName("aggregate_id")
                .IsRequired();

            builder.Property(x => x.Payload)
                .HasColumnName("payload")
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(x => x.OccurredAtUtc)
                .HasColumnName("occurred_at_utc")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.ProcessedAtUtc)
                .HasColumnName("processed_at_utc")
                .HasColumnType("timestamptz");

            builder.Property(x => x.Error)
                .HasColumnName("error")
                .HasMaxLength(1000);

            builder.Property(x => x.RetryCount)
                .HasColumnName("retry_count")
                .IsRequired();

            builder.HasIndex(x => x.ProcessedAtUtc);
            builder.HasIndex(x => x.OccurredAtUtc);
        }
    }
}
