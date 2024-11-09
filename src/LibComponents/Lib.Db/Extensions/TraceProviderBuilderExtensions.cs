using Npgsql;
using OpenTelemetry.Trace;

namespace Lib.Db.Extensions;

public static class TraceProviderBuilderExtensions
{
    public static TracerProviderBuilder AddDatabaseTelemetry(this TracerProviderBuilder traces) => traces.AddNpgsql();
}