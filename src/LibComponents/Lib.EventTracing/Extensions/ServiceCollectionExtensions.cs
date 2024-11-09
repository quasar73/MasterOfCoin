using System.Diagnostics;
using Lib.EventTracing.OpenTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Lib.EventTracing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTracing(this IServiceCollection services,
        string appName, string jaegerConnectionString, bool isLocalDevelopment, Action<TracerProviderBuilder>? configure = null)
    {
        services
            .AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .AddSource(appName)
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName: appName)
                        .AddAttributes(new Dictionary<string, object> { { "deployment.environment", "testapp" } }))
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.EnrichWithHttpRequest = EnrichWithHttpRequest;
                        options.EnrichWithHttpResponse = EnrichWithHttpResponse;
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation()
                    .SetErrorStatusOnException(true);

                // Additional telemetry registration
                configure?.Invoke(builder);

                // Local debug output to console
                if (isLocalDevelopment)
                {
                    ActivitySource.AddActivityListener(new ActivityListener()
                    {
                        ShouldListenTo = _ => true,
                        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
                    });
                    builder.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Debug);
                }

                builder
                    .AddOtlpExporter(options =>
                    {
                        var uri = new Uri(jaegerConnectionString);
                        options.Endpoint = uri;
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            });

        services.AddSingleton(new ActivitySource(appName));

        return services;
    }
    
    private static void EnrichWithHttpRequest(Activity activity, HttpRequest request)
    {
        var context = request.HttpContext;
        activity.AddTag(OpenTelemetryAttributeName.Http.Flavor, OpenTelemetryAttributeName.Http.GetHttpFlavour(request.Protocol));
        activity.AddTag(OpenTelemetryAttributeName.Http.ClientIP, context.Connection.RemoteIpAddress);
        activity.AddTag(OpenTelemetryAttributeName.Http.RequestContentLength, request.ContentLength);
        activity.AddTag(OpenTelemetryAttributeName.Http.RequestContentType, request.ContentType);

        var user = context.User;
        if (!string.IsNullOrWhiteSpace(user.Identity?.Name))
        {
            activity.AddTag(OpenTelemetryAttributeName.EndUser.Id, user.Identity.Name);
            activity.AddTag(OpenTelemetryAttributeName.EndUser.Scope, string.Join(',', user.Claims.Select(x => x.Value)));
        }
    }

    private static void EnrichWithHttpResponse(Activity activity, HttpResponse response)
    {
        activity.AddTag(OpenTelemetryAttributeName.Http.ResponseContentLength, response.ContentLength);
        activity.AddTag(OpenTelemetryAttributeName.Http.ResponseContentType, response.ContentType);
    }
}