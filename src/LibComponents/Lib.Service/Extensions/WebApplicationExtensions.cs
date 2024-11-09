using System.Reflection;
using Hangfire;
using Lib.Db.Extensions;
using Lib.EventTracing.Extensions;
using Lib.Scheduler.Extensions;
using Lib.Service.Settings;
using Lib.Service.Trace;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lib.Service.Extensions;

public static class WebApplicationExtensions
{
    public static void ConfigureBuilder(this WebApplicationBuilder builder, string serviceName, bool isLocalDevelopment,
        Action<SwaggerGenOptions>? configureSwagger, Assembly? migrationsAssembly, Assembly? controllersAssembly, 
        out IMvcBuilder mvcBuilder, out ConnectionStrings connectionStrings)
    {
        connectionStrings = new ConnectionStrings();
        var csSection = builder.Configuration.GetSection(nameof(ConnectionStrings)); 
        
        var loggerSettings = new Settings.Logger();
        var loggerSection = builder.Configuration.GetSection(nameof(Settings.Logger));

        loggerSection.Bind(loggerSettings);
        builder.Services.Configure<Settings.Logger>(loggerSection);
        
        var hcBuilder = builder.Services.AddHealthChecks().AddOpenApiDocument();

        // builder.WebHost.AddLogging(version, isLocalDevelopment, loggerSettings.Level);

        TryAddDatabase(builder, connectionStrings, hcBuilder, migrationsAssembly);
        
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
                
                if (!string.IsNullOrWhiteSpace(cs.SchedulerUri))
                {
                    bldr.AddSchedulerTelemetry();
                }
                
                // if (!string.IsNullOrWhiteSpace(cs.CacheUri))
                // {
                //     bldr.AddCacheTelemetry();
                // }
                //
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
            })
            .AddSignalR();
    }
    
    public static WebApplication ConfigureApp(this WebApplication app, bool isLocalDevelopment,
        string serviceName, List<Action<WebApplication>>? configureWebApplication)
    {
        if (isLocalDevelopment)
        {
            app.UseCors(cfg => cfg
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .AllowAnyOrigin()
                .SetIsOriginAllowed(_ => true)
                .SetIsOriginAllowedToAllowWildcardSubdomains());
        }

        app.UseRouting();

        configureWebApplication?.ForEach(c => c.Invoke(app));

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
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