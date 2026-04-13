using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Persistence.Configurations
{
    public sealed class ProviderRoutingRuleConfiguration : IEntityTypeConfiguration<ProviderRoutingRule>
    {
        public void Configure(EntityTypeBuilder<ProviderRoutingRule> builder)
        {
            builder.ToTable("provider_routing_rules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Provider)
                .HasColumnName("provider")
                .HasConversion<short>()
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasColumnName("priority")
                .IsRequired();

            builder.Property(x => x.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3);

            builder.Property(x => x.MinAmountMinor)
                .HasColumnName("min_amount_minor");

            builder.Property(x => x.MaxAmountMinor)
                .HasColumnName("max_amount_minor");

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.HasIndex(x => new { x.Provider, x.Priority });
        }
    }
}
