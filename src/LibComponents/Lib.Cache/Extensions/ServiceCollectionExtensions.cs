using Base.Cache.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using StackExchange.Redis.MultiplexerPool;

namespace Lib.Cache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheStore(this IServiceCollection services, string configuration, int poolSize, Action<IServiceProvider, ConfigurationOptions>? updateConfig = null)
    {
        services.TryAddSingleton<ICacheStore, RedisCacheStore>();
        services.TryAddSingleton(sp =>
        {
            var configurationOptions = ConfigurationOptions.Parse(configuration);
            updateConfig?.Invoke(sp, configurationOptions);

            return ConnectionMultiplexerPoolFactory.Create(
                poolSize: poolSize,
                configurationOptions: configurationOptions,
                connectionSelectionStrategy: ConnectionSelectionStrategy.RoundRobin);
        });
        services.TryAddSingleton<IDatabaseProvider, MultiplexerPoolDatabaseProvider>();
        return services;
    }
}