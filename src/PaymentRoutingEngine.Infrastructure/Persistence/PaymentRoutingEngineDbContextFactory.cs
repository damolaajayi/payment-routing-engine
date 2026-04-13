using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Persistence
{
    public sealed class PaymentRoutingEngineDbContextFactory
    : IDesignTimeDbContextFactory<PaymentRoutingEngineDbContext>
    {
        public PaymentRoutingEngineDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PaymentRoutingEngineDbContext>();

            var connectionString =
                "Host=localhost;Port=5432;Database=payment_routing_engine;Username=db-user;Password=adedamolaajayi";

            optionsBuilder.UseNpgsql(connectionString);

            return new PaymentRoutingEngineDbContext(optionsBuilder.Options);
        }
    }
}
