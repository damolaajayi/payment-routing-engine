using Microsoft.Extensions.Diagnostics.HealthChecks;
using PaymentRoutingEngine.Infrastructure.Messaging;

namespace PaymentRoutingEngine.Api.HealthChecks;

public sealed class RabbitMqConnectionHealthCheck : IHealthCheck
{
    private readonly RabbitMqConnectionProvider _connectionProvider;

    public RabbitMqConnectionHealthCheck(RabbitMqConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

            return connection.IsOpen
                ? HealthCheckResult.Healthy("RabbitMQ connection is open.")
                : HealthCheckResult.Unhealthy("RabbitMQ connection is closed.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ connection failed.", exception);
        }
    }
}
