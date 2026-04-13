using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentRoutingEngine.Application.Abstractions.Common;
using PaymentRoutingEngine.Application.Abstractions.Persistence;
using PaymentRoutingEngine.Application.Abstractions.Providers;
using PaymentRoutingEngine.Infrastructure.Persistence;
using PaymentRoutingEngine.Infrastructure.Persistence.Repositories;
using PaymentRoutingEngine.Infrastructure.Providers.MockProvider;
using PaymentRoutingEngine.Infrastructure.Time;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Database")
                ?? throw new InvalidOperationException("Database connection string is missing.");

            services.AddDbContext<PaymentRoutingEngineDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
            services.AddScoped<IPaymentAttemptRepository, PaymentAttemptRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
            services.AddScoped<IPaymentProviderClient, MockProviderClient>();

            return services;
        }
    }
}
