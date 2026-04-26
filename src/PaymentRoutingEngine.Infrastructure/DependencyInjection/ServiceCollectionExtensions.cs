using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentRoutingEngine.Application.Abstractions.Common;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Application.Abstractions.Payments;
using PaymentRoutingEngine.Application.Abstractions.Persistence;
using PaymentRoutingEngine.Application.Abstractions.Providers;
using PaymentRoutingEngine.Infrastructure.Messaging;
using PaymentRoutingEngine.Infrastructure.Messaging.Behaviors;
using PaymentRoutingEngine.Infrastructure.Persistence;
using PaymentRoutingEngine.Infrastructure.Persistence.Repositories;
using PaymentRoutingEngine.Infrastructure.Providers;
using PaymentRoutingEngine.Infrastructure.Providers.MockProvider;
using PaymentRoutingEngine.Infrastructure.Providers.Paystack;
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

            services.AddHttpClient<PaystackProviderClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.paystack.co/");
            });

            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
            services.AddScoped<IPaymentAttemptRepository, PaymentAttemptRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
            services.AddScoped<IPaymentProviderClient, PaystackProviderClient>();
            services.AddScoped<IPaymentProviderClient, MockProviderClient>();
            services.AddScoped<IPaymentProviderRoutingService, PaymentProviderRoutingService>();
            services.AddScoped<IPaymentRetryPolicy, PaymentRetryPolicy>();
            services.AddScoped<IPaymentProviderResolver, PaymentProviderResolver>();
            services.AddScoped<IDispatcher, Dispatcher>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));

            services.AddSingleton<RabbitMqConnectionProvider>();
            services.AddScoped<IPaymentProcessingQueue, RabbitMqPaymentProcessingQueue>();

            return services;
        }
    }
}
