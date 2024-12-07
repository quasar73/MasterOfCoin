using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace Lib.MessageBroker.Services;

public class MessageBrokerHealthCheck(ObjectPool<IModel> pool, ILogger<MessageBrokerHealthCheck> logger) : IHealthCheck
{
    private static readonly Task<HealthCheckResult> Healthy = Task.FromResult(HealthCheckResult.Healthy());

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var openedChannelFound = false;

            do
            {
                var channel = pool.Get(); 
            
                openedChannelFound = channel.IsOpen;
                
                pool.Return(channel); //to dispose it properly
            }
            while (!openedChannelFound);

            return Healthy;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhealthy");
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
        }
    }

}
