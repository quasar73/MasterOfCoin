using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using StackExchange.Redis.MultiplexerPool;

namespace Lib.Cache.Extensions;

public static class TraceProviderBuilderExtensions
{
    public static TracerProviderBuilder AddCacheTelemetry(this TracerProviderBuilder builder)
    {
        if (builder is not IDeferredTracerProviderBuilder deferredTracerProviderBuilder)
        {
            throw new NotSupportedException("StackExchange.Redis telemetry should be used with dependency injection. To enable dependency injection use the OpenTelemetry.Extensions.Hosting package");
        }

        return deferredTracerProviderBuilder.Configure((sp, b) =>
        {
            var connections = new List<IConnectionMultiplexer>();
            var pool = sp.GetRequiredService<IConnectionMultiplexerPool>();
            while (connections.Count != pool.PoolSize)
            {
                var mx = pool.GetAsync().GetAwaiter().GetResult().Connection;
                if (!connections.Contains(mx))
                {
                    connections.Add(mx);
                }
            }

            foreach (var connection in connections)
            {
                b.AddRedisInstrumentation(connection, options =>
                {
                    options.Enrich = (activity, _) =>
                        activity.AddTag("connection.instance", connection.GetHashCode());
                });
            }
        });
    }
}