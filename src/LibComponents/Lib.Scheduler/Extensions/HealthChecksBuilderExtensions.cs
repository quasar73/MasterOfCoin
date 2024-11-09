using Microsoft.Extensions.DependencyInjection;

namespace Lib.Scheduler.Extensions;

public static class HealthChecksBuilderExtensions
{
    public const string Tag = "SchedulerHealthCheck";

    public static IHealthChecksBuilder AddSchedulerHealthCheck(this IHealthChecksBuilder healthChecksBuilder, string connectionString) =>
        healthChecksBuilder.AddNpgSql(connectionString, name: Tag, tags: new[] { Tag });
}
