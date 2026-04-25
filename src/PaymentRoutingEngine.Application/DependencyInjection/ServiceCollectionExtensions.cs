using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentRoutingEngine.Application.Abstractions.Common;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Application.Abstractions.Persistence;
using PaymentRoutingEngine.Application.Abstractions.Providers;
using PaymentRoutingEngine.Application.Behaviors;
using PaymentRoutingEngine.Application.Payments.CreatePayment;
using PaymentRoutingEngine.Application.Payments.GetPaymentById;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<
                IRequestHandler<CreatePaymentCommand, CreatePaymentResult>,
                CreatePaymentCommandHandler>();
            services.AddValidatorsFromAssembly(typeof(CreatePaymentCommandValidator).Assembly);
            services.AddScoped<
                IRequestHandler<GetPaymentByIdQuery, GetPaymentByIdResult>,
                GetPaymentByIdQueryHandler>();

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            return services;
        }
    }
}
