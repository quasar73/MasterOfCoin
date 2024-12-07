using Lib.MessageBroker.Contracts;
using Lib.MessageBroker.Services;
using Lib.MessageBroker.Services.Decorators;
using Lib.MessageBroker.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Base.ServiceTemplate.Tests")]
[assembly: InternalsVisibleTo("Lib.MessageBroker.Tests")]

namespace Lib.MessageBroker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection services, string appName, string connectionString, out Type[] consumers, params Assembly[] consumersAssemblies)
    {
        var connectionFactory = new ConnectionFactory() { Uri = new Uri(connectionString), DispatchConsumersAsync = true };

        services
            .TryAddActivitySourceHolder(appName)
            .AddSingleton(_ => connectionFactory.CreateConnection())
            .AddSingleton<IPublisher, Publisher>()
            .Decorate<IPublisher, PublisherRetryDecorator>()
            .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
            .AddSingleton(sp =>
            {
                var provider = sp.GetRequiredService<ObjectPoolProvider>();
                var connection = sp.GetRequiredService<IConnection>();

                return provider.Create(new ModelPooledObjectPolicy(connection));
            });

        if (consumersAssemblies.Length > 0)
        {
            services.RegisterConsumerTypes(consumersAssemblies, out consumers);
        }
        else
        {
            consumers = [];
        }

        return services;
    }

    private static IServiceCollection RegisterConsumerTypes(this IServiceCollection services, Assembly[] consumersAssemblies, out Type[] consumers)
    {
        consumers = consumersAssemblies
            .SelectMany(asm => asm.GetTypes()
                .Where(t => !t.IsAbstract && t.IsConsumer()))
            .ToArray();

        foreach (var consumer in consumers)
        {
            services.AddSingleton(consumer);
        }

        return services;
    }

    private static bool IsConsumer(this Type type) =>
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>));

    public static IDisposable RegisterConsumers(this IServiceProvider services, Type[] consumers) =>
        new MessageBrokerSubscription(
            consumers,
            services.GetRequiredService<IConnection>(),
            services,
            services.GetRequiredService<ILogger<MessageBrokerSubscription>>(),
            services.GetRequiredService<ActivitySourceHolder>());

    private static IServiceCollection TryAddActivitySourceHolder(this IServiceCollection services, string appName)
    {
        services.TryAddSingleton(sp => new ActivitySourceHolder(appName));
        return services;
    }
}
