using System.Globalization;
using System.Reflection;
using Lib.Cache.Extensions;
using Lib.CrossService.Extensions;
using Lib.CrossService.Models;
using Lib.Db.Extensions;
using Lib.EventTracing.Extensions;
using Lib.Logger.Extensions;
using Lib.Scheduler.Extensions;
using Lib.Service.Settings;
using Lib.Service.Trace;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lib.Service.Extensions;

public static class WebApplicationExtensions
{
    public static void ConfigureBuilder(this WebApplicationBuilder builder, string serviceName, bool isLocalDevelopment,
        Action<ConnectionStrings>? configureConnectionStrings, Action<SwaggerGenOptions>? configureSwagger, Assembly? migrationsAssembly, 
        Assembly[] grpcServicesAssemblies, Assembly[] grpcClientsAssemblies, ServerConnectionSettings? grpcServerConnectionSettings, Assembly? controllersAssembly, 
        out IMvcBuilder mvcBuilder, out ConnectionStrings connectionStrings)
    {
        connectionStrings = new ConnectionStrings();
        var csSection = builder.Configuration.GetSection(nameof(ConnectionStrings)); 
        
        csSection.Bind(connectionStrings);
        builder.Services.Configure<ConnectionStrings>(csSection);

        if (configureConnectionStrings != null)
        {
            configureConnectionStrings.Invoke(connectionStrings);
            builder.Services.Configure(configureConnectionStrings);
        }
        
        var loggerSettings = new Settings.Logger();
        var loggerSection = builder.Configuration.GetSection(nameof(Settings.Logger));

        loggerSection.Bind(loggerSettings);
        builder.Services.Configure<Settings.Logger>(loggerSection);
        
        var schedulerSettings = new Settings.Scheduler();
        var schedulerSection = builder.Configuration.GetSection(nameof(Settings.Scheduler));

        schedulerSection.Bind(schedulerSettings);
        builder.Services.Configure<Settings.Scheduler>(schedulerSection);
        
        var grpcConnectionSettings = grpcServerConnectionSettings;
        if (grpcServerConnectionSettings == null)
        {
            var grpcConnectionSection = builder.Configuration.GetSection(nameof(ServerConnectionSettings));
            grpcConnectionSettings = new ServerConnectionSettings();
            grpcConnectionSection.Bind(grpcConnectionSettings);
        }
        
        var hcBuilder = builder.Services.AddHealthChecks().AddOpenApiDocument();

        builder.WebHost.AddLogging(isLocalDevelopment, loggerSettings.Level);

        TryAddDatabase(builder, connectionStrings, hcBuilder, migrationsAssembly);
        
        if (!string.IsNullOrWhiteSpace(connectionStrings.CacheUri))
        {
            var shouldUseHeartbeatConsistencyChecks  = TimeSpan.TryParse(Environment.GetEnvironmentVariable("REDIS_HEARTBEAT_INTERVAL"), CultureInfo.InvariantCulture, out var heartbeatInterval);
            var redisPoolSize  = int.TryParse(Environment.GetEnvironmentVariable("REDIS_CONNECTION_POOL_SIZE"), out var value) ? value : 1;
            builder.Services.AddCacheStore(connectionStrings.CacheUri, redisPoolSize, (sp, redisOptions) =>
            {
                redisOptions.LoggerFactory = sp.GetService<ILoggerFactory>();
                if (shouldUseHeartbeatConsistencyChecks)
                {
                    // this option more suitable for "streaming" connections. Allows always sending keepalive checks even if a connection isn’t idle.
                    redisOptions.HeartbeatConsistencyChecks = true;
                    redisOptions.HeartbeatInterval = heartbeatInterval;
                }
            });
        }
        
        if (grpcServicesAssemblies.Length != 0)
        {
            builder.Services.AddGrpcServices(isLocalDevelopment, grpcServicesAssemblies);
        }
        
        if (!string.IsNullOrWhiteSpace(connectionStrings.GrpcGatewayUri))
        {
            builder.Services.AddGrpcClients(connectionStrings.GrpcGatewayUri, grpcConnectionSettings!, grpcClientsAssemblies);
        }
        
        if (!string.IsNullOrWhiteSpace(connectionStrings.TracingUri))
        {
            var probability = double.TryParse(Environment.GetEnvironmentVariable("TRACING_VOLUME"), out var volume) ? volume : 0.1; 
            builder.Services.AddSingleton(new TraceSampler(probability));
            builder.Services.AddSingleton<ITracingConfiguration, TracingConfiguration>();
            var cs = connectionStrings;
            builder.Services.AddTracing(serviceName, connectionStrings.TracingUri, isLocalDevelopment, bldr =>
            {
                bldr.SetSampler<TraceSampler>();

                if (!string.IsNullOrWhiteSpace(cs.DbUri))
                {
                    bldr.AddDatabaseTelemetry();
                }
                
                // if (!string.IsNullOrWhiteSpace(cs.CacheUri))
                // {
                //     bldr.AddCacheTelemetry();
                // }
                
                if (!string.IsNullOrWhiteSpace(cs.SchedulerUri))
                {
                    bldr.AddSchedulerTelemetry();
                }
                
                // if (!string.IsNullOrWhiteSpace(cs.MessageBrokerUri))
                // {
                //     bldr.AddMessageBrokerTelemetry();
                // }
                //
                // if (!string.IsNullOrWhiteSpace(cs.DistributedLockUri))
                // {
                //     bldr.AddDistributedLockTelemetry();
                // }
                
                // //Register telemetry for grpc every time
                // bldr.AddGrpcTelemetry();
            });
        }

        mvcBuilder = builder.Services.AddControllers();
        if (controllersAssembly != default)
        {
            mvcBuilder.AddApplicationPart(controllersAssembly);
        }

        builder.Services
            .AddValidators(controllersAssembly)
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                var internalServiceEndpoint = Environment.GetEnvironmentVariable("INTERNAL_SERVICE_ENDPOINT");
                if (!string.IsNullOrEmpty(internalServiceEndpoint))
                {
                    options.AddServer(new() { Url = internalServiceEndpoint });
                }
                
                options.MapType<TimeSpan>(() => new OpenApiSchema
                {
                    Type = "string",
                    Example = new OpenApiString("00:05:00")
                });

                options.SupportNonNullableReferenceTypes();
                configureSwagger?.Invoke(options);
                
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            })
            .AddSignalR();
    }
    
    public static WebApplication ConfigureApp(this WebApplication app, Assembly[] grpcServicesAssemblies, 
        bool isLocalDevelopment, string serviceName, List<Action<WebApplication>>? configureWebApplication)
    {
        if (isLocalDevelopment)
        {
            app.UseCors(cfg => cfg
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .SetIsOriginAllowed(_ => true)
                .SetIsOriginAllowedToAllowWildcardSubdomains());
        }

        app.UseRouting();
        
        if (grpcServicesAssemblies.Length != 0)
        {
            app.MapGrpcServices(isLocalDevelopment);
        }

        configureWebApplication?.ForEach(c => c.Invoke(app));

#pragma warning disable ASP0014
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
#pragma warning restore ASP0014
        app.UseSwagger();
        app.UseSwaggerUI(c =>
            {
                var internalServiceEndpoint = Environment.GetEnvironmentVariable("INTERNAL_SERVICE_ENDPOINT");
                if (!string.IsNullOrEmpty(internalServiceEndpoint))
                {
                    c.SwaggerEndpoint($"{internalServiceEndpoint}/swagger/v1/swagger.json", serviceName);
                }
            });

        return app;
    }
    
    private static void TryAddDatabase(WebApplicationBuilder builder,
        ConnectionStrings connectionStrings,
        IHealthChecksBuilder hcBuilder,
        Assembly? migrationsAssembly)
    {
        if (!string.IsNullOrWhiteSpace(connectionStrings.DbUri))
        {
            var parsedCs = new NpgsqlConnectionStringBuilder(connectionStrings.DbUri);
            if (connectionStrings.DbEnableMultiplexing && !parsedCs.Multiplexing)
            {
                parsedCs.Multiplexing = true;
                parsedCs.KeepAlive = 0;
            }
    
            builder.Services.AddDatabase(
                connectionString: parsedCs.ToString(),
                masterConnectionString: connectionStrings.DbMasterUri,
                migrationsAssembly: null);
    
            if (migrationsAssembly is not null)
            {
                var mCs = connectionStrings.DbUri;
                builder.Services.AddDatabaseMigrator(mCs, migrationsAssembly);
            }
    
            // hcBuilder.AddDatabaseHealthCheck(connectionStrings.DbUri);
        }
    }
}