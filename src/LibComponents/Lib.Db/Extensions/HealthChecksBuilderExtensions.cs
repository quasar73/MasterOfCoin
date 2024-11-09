using Microsoft.Extensions.DependencyInjection;

namespace Lib.Db.Extensions;

public static class HealthChecksBuilderExtensions
{
    public const string Tag = "DatabaseHealthCheck";

    public static IHealthChecksBuilder AddDatabaseHealthCheck(this IHealthChecksBuilder healthChecksBuilder, string connectionString) =>
        healthChecksBuilder.AddNpgSql(sp => sp.GetRequiredService<IDataSourceHolder>().GetDataSource(connectionString), name: Tag, tags: [Tag]);
}