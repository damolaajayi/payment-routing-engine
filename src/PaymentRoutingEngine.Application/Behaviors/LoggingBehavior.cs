using Microsoft.Extensions.Logging;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Behaviors
{
    public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            Func<Task<TResponse>> next)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogInformation("Handling request {RequestName}", requestName);

            var response = await next();

            _logger.LogInformation("Handled request {RequestName}", requestName);

            return response;
        }
    }
}
