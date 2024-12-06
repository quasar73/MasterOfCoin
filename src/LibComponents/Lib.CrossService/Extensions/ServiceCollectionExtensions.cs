using Lib.CrossService.Client;
using Lib.CrossService.Interfaces;
using Lib.CrossService.Metrics;
using Lib.CrossService.Models;
using Lib.CrossService.Server;
using Lib.CrossService.Server.Interceptors;
using Lib.CrossService.Tracing;
using Lib.CrossService.Utils;
using Castle.DynamicProxy;
using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Grpc.Health.V1;

[assembly: InternalsVisibleTo("Lib.CrossService.Tests")]

namespace Lib.CrossService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string MapGrpcServiceMethod = "MapGrpcService";

        public static IServiceCollection AddGrpcClients(this IServiceCollection services, string gateway, ServerConnectionSettings settings, params Assembly[] assemblies)
        {
            return services.AddGrpcClients(_ => gateway, settings, assemblies);
        }

        public static IServiceCollection AddGrpcClients(this IServiceCollection services, Func<IServiceProvider, string> getGatewayFunc, ServerConnectionSettings settings, params Assembly[] assemblies)
        {
            if (int.TryParse(Environment.GetEnvironmentVariable("DNS_RESOLVER_TIMEOUT_SEC"), out var dnsResolverTimeout))
            {
                services.AddSingleton<ResolverFactory>(_ => new DnsResolverFactory(TimeSpan.FromSeconds(dnsResolverTimeout)));
            }
            
            if (!int.TryParse(Environment.GetEnvironmentVariable("DEFAULT_GRPC_PORT"), out var defaultPort))
            {
                defaultPort = 80;
            }

            if (settings?.RequestTimeMetricsLimit is not null)
            {
                GrpcMetricsService.Log.SetRequestTimeMetricsLimit(settings.RequestTimeMetricsLimit.Value);
            }

            services.AddSingleton<IDnsWrapper, DnsWrapper>();

            var genericType = typeof(IGrpcClient<>);
            var mapExp = from type in assemblies.SelectMany(a => a.GetExportedTypes())
                         where !type.IsAbstract && !type.IsGenericTypeDefinition
                         let interfaces = type.GetInterfaces()
                         let genericInterfaces = interfaces.Where((i) => i.IsGenericType && i.GetGenericTypeDefinition() == genericType)
                         let matchingInterface = genericInterfaces.FirstOrDefault()
                         where matchingInterface != null
                         select new
                         {
                             ClientType = type,
                             InterfaceType = matchingInterface.GetGenericArguments()[0]
                         };

            var map = mapExp.ToList();

            if (map.Any())
            {
                services.TryAddSingleton<ProxyGenerator>();
                services.TryAddSingleton<IChannelResolver>(new DefaultChannelResolver(settings?.MapToGatewayRoot ?? false));
                services.TryAddActivitySourceHolder();

                map.ForEach(ci =>
                {
                    services.AddSingleton(ci.InterfaceType, sp =>
                    {
                        var generator = sp.GetRequiredService<ProxyGenerator>();
                        var resolver = sp.GetRequiredService<IChannelResolver>();
                        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                        var channel = resolver.ResolveChannel(getGatewayFunc(sp), defaultPort, ci.InterfaceType, settings?.UseLoadBalancing ?? false, loggerFactory);

                        var constructor = ci.ClientType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, [typeof(CallInvoker)])
                                        ?? throw new MissingMethodException($"Can't find constructor {ci.ClientType}(CallInvoker)");
                        var client = constructor.Invoke(new[] { channel.Invoker });
                        var interceptor = new GrpcClientInterceptor(
                            ci.InterfaceType,
                            ci.ClientType,
                            client,
                            channel.TargetHostUri,
                            sp.GetService<ActivitySourceHolder>,
                            sp.GetRequiredService<ILogger<GrpcClientInterceptor>>(),
                            sp.GetRequiredService<IDnsWrapper>());

                        return generator.CreateInterfaceProxyWithoutTarget(ci.InterfaceType, interceptor.ToInterceptor());
                    });
                });
            }

            return services;
        }

        public static IServiceCollection AddGrpcServices(this IServiceCollection services, bool isDevelopment, params Assembly[] assemblies)
        {
            var genericType = typeof(IGrpcService<>);

            var mapExp = from type in assemblies.SelectMany(a => a.GetExportedTypes())
                         where !type.IsAbstract && !type.IsGenericTypeDefinition
                         let interfaces = type.GetInterfaces()
                         let genericInterfaces = interfaces.Where((i) => i.IsGenericType && i.GetGenericTypeDefinition() == genericType)
                         let matchingInterface = genericInterfaces.FirstOrDefault()
                         where matchingInterface != null
                         select new
                         {
                             ServiceType = type,
                             InterfaceType = matchingInterface.GetGenericArguments()[0]
                         };

            var map = mapExp.ToList();

            services.AddGrpc(options =>
            {
                options.Interceptors.Add<ServerExceptionInterceptor>();
            });

            services.AddGrpcHealthChecks()
                .AddCheck(nameof(Health.HealthClient.Check), () => HealthCheckResult.Healthy());

            services.TryAddSingleton<IObjectCache, ObjectCache>();
            services.TryAddActivitySourceHolder();

            if (isDevelopment)
            {
                services.AddGrpcReflection();
            }

            return services
                .AddSingleton<IInterceptorSelector>(sp => new GrpcServiceSelector(map.Select(tt => tt.ServiceType), sp.GetRequiredService<ActivitySourceHolder>()))
                .AddSingleton<IInterceptor[]>(sp =>
                {
                    return map
                        .Select(tt => new GrpcServiceInterceptor(
                            tt.ServiceType,
                            tt.InterfaceType,
                            sp.GetRequiredService(tt.InterfaceType),
                            sp.GetRequiredService<ActivitySourceHolder>,
                            sp.GetRequiredService<ILogger<GrpcServiceInterceptor>>()).ToInterceptor())
                        .ToArray();
                });
        }

        public static void MapGrpcServices(this IEndpointRouteBuilder builder, bool isDevelopment = false)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (isDevelopment)
            {
                builder.MapGrpcReflectionService();
            }

            var methods = typeof(GrpcEndpointRouteBuilderExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public);
            var mapGrpcMethod = Array.Find(methods, m => m.Name == MapGrpcServiceMethod)
                ?? throw new InvalidCastException($"Can't find {MapGrpcServiceMethod} method");

            builder.ServiceProvider.EnumerateGrpcServices()
                .Select(proxyType => mapGrpcMethod.MakeGenericMethod(proxyType)) //MapGrpcService<TestGrpcServiceProxy>(IEndpointRouteBuilder builder)
                .ToList()
                .ForEach(mapGrpcMethod => mapGrpcMethod.Invoke(builder, [builder]));

            builder.MapGrpcHealthChecksService();
        }

        internal static IEnumerable<Type> EnumerateGrpcServices(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (serviceProvider.GetService<IInterceptorSelector>() is not GrpcServiceSelector selector)
            {
                //No GrpcServiceSelector injected, let's quit
                return [];
            }

            var proxyBuilder = new DefaultProxyBuilder();
            var options = new ProxyGenerationOptions
            {
                Selector = selector
            };

            return selector.GrpcServiceTypes
                .Select(initialType => proxyBuilder.CreateClassProxyType(initialType, null, options));
        }

        private static void TryAddActivitySourceHolder(this IServiceCollection services)
            => services.TryAddSingleton(sp => new ActivitySourceHolder(sp.GetService<ActivitySource>()));
    }
}
