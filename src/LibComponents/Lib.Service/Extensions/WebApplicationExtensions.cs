using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lib.Service.Extensions;

public static class WebApplicationExtensions
{
    public static void ConfigureBuilder(this WebApplicationBuilder builder, string serviceName, bool isLocalDevelopment,
        Action<SwaggerGenOptions>? configureSwagger, Assembly? migrationsAssembly, Assembly? controllersAssembly, out IMvcBuilder mvcBuilder)
    {
        var loggerSettings = new Settings.Logger();
        var loggerSection = builder.Configuration.GetSection(nameof(Settings.Logger));

        loggerSection.Bind(loggerSettings);
        builder.Services.Configure<Settings.Logger>(loggerSection);

        // builder.WebHost.AddLogging(version, isLocalDevelopment, loggerSettings.Level);

        // TryAddDatabase(builder, connectionStrings, hcBuilder, migrationsAssembly);

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

        app.UseEndpoints(endpoints => endpoints.MapControllers())
            .UseSwagger()
            .UseSwaggerUI(c =>
            {
                var internalServiceEndpoint = Environment.GetEnvironmentVariable("INTERNAL_SERVICE_ENDPOINT");
                if (!string.IsNullOrEmpty(internalServiceEndpoint))
                {
                    c.SwaggerEndpoint($"{internalServiceEndpoint}/swagger/v1/swagger.json", serviceName);
                }
            });

        return app;
    }
    
    // private static void TryAddDatabase(WebApplicationBuilder builder,
    //     ConnectionStrings connectionStrings,
    //     IHealthChecksBuilder hcBuilder,
    //     Assembly? migrationsAssembly)
    // {
    //     if (!string.IsNullOrWhiteSpace(connectionStrings.DbUri))
    //     {
    //         var parsedCs = new NpgsqlConnectionStringBuilder(connectionStrings.DbUri);
    //         if (connectionStrings.DbEnableMultiplexing && !parsedCs.Multiplexing)
    //         {
    //             parsedCs.Multiplexing = true;
    //             parsedCs.KeepAlive = 0;
    //         }
    //
    //         builder.Services.AddDatabase(
    //             connectionString: parsedCs.ToString(),
    //             masterConnectionString: connectionStrings.DbMasterUri,
    //             migrationsAssembly: null);
    //
    //         if (migrationsAssembly is not null)
    //         {
    //             var mCs = connectionStrings.DbUri;
    //             builder.Services.AddDatabaseMigrator(mCs, migrationsAssembly);
    //         }
    //
    //         hcBuilder.AddDatabaseHealthCheck(connectionStrings.DbUri);
    //     }
    // }
}