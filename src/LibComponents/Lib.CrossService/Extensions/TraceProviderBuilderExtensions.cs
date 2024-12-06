using Lib.CrossService.Tracing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Lib.CrossService.Extensions
{
    public static class TraceProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddGrpcTelemetry(this TracerProviderBuilder builder)
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
}
