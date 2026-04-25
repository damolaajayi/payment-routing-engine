using Microsoft.Extensions.DependencyInjection;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Infrastructure.Messaging
{
    public sealed class Dispatcher : IDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public Dispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            return Dispatch((dynamic)request, cancellationToken);
        }

        private Task<TResponse> Dispatch<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken)
            where TRequest : IRequest<TResponse>
        {
            var handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

            var behaviors = _serviceProvider
                .GetServices<IPipelineBehavior<TRequest, TResponse>>()
                .Reverse()
                .ToList();

            Func<Task<TResponse>> next = () => handler.Handle(request, cancellationToken);

            foreach (var behavior in behaviors)
            {
                var currentNext = next;
                next = () => behavior.Handle(request, cancellationToken, currentNext);
            }

            return next();
        }
    }
}
