using System.Reflection;
using Lib.CrossService.Models;
using Lib.Logger.Extensions;
using Lib.Service.Extensions;
using Lib.Service.Migrations.Interfaces;
using Lib.Service.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lib.Service;

public class ServiceBuilder
{
    internal readonly WebApplicationBuilder builder;
    internal readonly string serviceName;
    internal readonly bool isLocalDevelopment;
    internal Action<ConnectionStrings>? configureConnectionStrings;
    internal List<Action<WebApplication>>? configureWebApplication;
    internal Action<IMvcBuilder>? configureMvc;
    internal Action<SwaggerGenOptions>? configureSwagger;
    internal Assembly? migrationsAssembly;
    internal Assembly? controllersAssembly;
    internal Assembly[] grpcServicesAssemblies = [];
    internal Assembly[] grpcClientsAssemblies = [];
    internal Assembly[] consumersAssemblies = [];
    internal ServerConnectionSettings? grpcConnectionSettings;
    // internal HashSet<string>? acceptableComponents = [];
    
    internal static int DefaultHttp1Port = 80;
    internal static int DefaultHttp2Port = 8080;
    
    private ServiceBuilder(string serviceName, bool isLocalDevelopment)
    {
        ThreadPool.GetMinThreads(out var minwrk, out var minio);
        Console.WriteLine($"Thread pool initial min settings: wrk={minwrk}, io={minio}");
        if (ushort.TryParse(Environment.GetEnvironmentVariable("MIN_THREADS_POOL"), out var minthreadsOverride))
        {
            Console.WriteLine("minthreadsOverride = " + minthreadsOverride);
            ThreadPool.SetMinThreads(minthreadsOverride, minthreadsOverride * 2);
            ThreadPool.GetMinThreads(out var wrk, out var io);
            Console.WriteLine($"Thread pool tweaked min settings: wrk={wrk}, io={io}");
        }
        ThreadPool.GetMaxThreads(out var maxwrk, out var maxio);
        Console.WriteLine($"Thread pool initial max settings: wrk={maxwrk}, io={maxio}");
        if (ushort.TryParse(Environment.GetEnvironmentVariable("MAX_THREADS_POOL"), out var maxthreadsOverride))
        {
            Console.WriteLine("maxthreadsOverride = " + maxthreadsOverride);
            ThreadPool.SetMaxThreads(maxthreadsOverride, maxthreadsOverride * 2);
            ThreadPool.GetMaxThreads(out var wrk, out var io);
            Console.WriteLine($"Thread pool tweaked max settings: wrk={wrk}, io={io}");
        }

        builder = WebApplication.CreateBuilder();
        this.serviceName = serviceName;
        this.isLocalDevelopment = isLocalDevelopment;
    }
    
    public static ServiceBuilder CreateNewService(string serviceName, bool isLocalDevelopment) => new(serviceName, isLocalDevelopment);
    
    public ServiceBuilder AddControllers(Assembly controllersAssembly)
    {
        this.controllersAssembly = controllersAssembly;
        return this;
    }
    
    public ServiceBuilder AddMigrations(Assembly migrationsAssembly)
    {
        this.migrationsAssembly = migrationsAssembly;
        return this;
    }
    
    public ServiceBuilder AddServices(Action<IServiceCollection> configureServices)
    {
        configureServices(builder.Services);
        return this;
    }
    
    public ServiceBuilder AddServices(Action<IServiceCollection, IConfiguration> configureServices)
    {
        configureServices(builder.Services, builder.Configuration);
        return this;
    }
    
    public ServiceBuilder AddOptions<TOptions>(string? sectionName = default) where TOptions : class
    {
        builder.Services.Configure<TOptions>(builder.Configuration.GetSection(sectionName ?? typeof(TOptions).Name));
        return this;
    }
    
    public ServiceBuilder AddGrpcServices(params Assembly[] grpcServicesAssemblies)
    {
        this.grpcServicesAssemblies = grpcServicesAssemblies;
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(DefaultHttp2Port, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
            serverOptions.ListenAnyIP(DefaultHttp1Port, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1;
            });
        });

        return this;
    }

    public ServiceBuilder AddGrpcClients(params Assembly[] grpcClientsAssemblies)
    {
        this.grpcClientsAssemblies = grpcClientsAssemblies;
        return this;
    }

    public ServiceBuilder AddGrpcClients(ServerConnectionSettings grpcConnectionSettings, params Assembly[] grpcClientsAssemblies)
    {
        this.grpcConnectionSettings = grpcConnectionSettings;
        this.grpcClientsAssemblies = grpcClientsAssemblies;
        return this;
    }
    
    public ServiceBuilder AddConnectionStrings(Action<ConnectionStrings> configure)
    {
        configureConnectionStrings = configure;
        return this;
    }
    
    public ServiceBuilder AddConsumers(params Assembly[] consumersAssemblies)
    {
        this.consumersAssemblies = consumersAssemblies;
        return this;
    }
    
    public ServiceBuilder ConfigureWebApp(Action<WebApplication> configure)
    {
        configureWebApplication ??= [];
        configureWebApplication.Add(configure);

        return this;
    }
    
    public async Task StartAsync()
    {
        var app = InitWebApp();

        await app.RunAsync();
    }
    
    internal WebApplication InitWebApp()
    {
        builder.ConfigureBuilder(serviceName, isLocalDevelopment, configureConnectionStrings, configureSwagger, migrationsAssembly,
            grpcServicesAssemblies, consumersAssemblies, grpcClientsAssemblies, controllersAssembly, grpcConnectionSettings, 
            out var mvcBuilder, out var connectionString, out var consumers);

        configureMvc?.Invoke(mvcBuilder);

        var app = builder
            .Build()
            .ConfigureApp(consumers, grpcServicesAssemblies, isLocalDevelopment, serviceName, configureWebApplication);

        app.Lifetime.ApplicationStarted.Register(async () =>
        {
            try
            {
                RunMigrations(app.Services);
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<ServiceBuilder>>();
                logger.Error("Application was stopped because of exception in migration", default, ex);
                await app.StopAsync();
            }
        });

        return app;
    }
    
    private static void RunMigrations(IServiceProvider serviceProvider)
    {
        var migrators = serviceProvider.GetServices<IMigrator>();
        Parallel.ForEach(migrators, migrator => migrator.MigrateUp());
    }
}