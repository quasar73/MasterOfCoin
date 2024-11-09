using Npgsql;
using OpenTelemetry.Trace;

namespace Lib.Scheduler.Extensions;

public static class TraceProviderBuilderExtensions
{
    public static TracerProviderBuilder AddSchedulerTelemetry(this TracerProviderBuilder traces)
    {
        ServiceCollectionExtensions.ActivitySourceHolder?.Enable();
        return traces.AddNpgsql();
    }
}
