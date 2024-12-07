using Lib.MessageBroker.Tracing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Lib.MessageBroker.Extensions;

public static class TraceProviderBuilderExtensions
{
    public static TracerProviderBuilder AddMessageBrokerTelemetry(this TracerProviderBuilder builder)
    {
        if (builder is IDeferredTracerProviderBuilder deferredTracerProviderBuilder)
        {
            return deferredTracerProviderBuilder.Configure(delegate (IServiceProvider sp, TracerProviderBuilder builder)
            {
                sp.GetService<ActivitySourceHolder>()?.Enable();
            });
        }

        return builder;
    }
}
