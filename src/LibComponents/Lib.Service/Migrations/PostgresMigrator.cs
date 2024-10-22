using System.Reflection;
using Lib.Db.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.Service.Migrations;

internal class PostgresMigrator : BaseMigrator
{
    internal PostgresMigrator(string connectionString,
        Assembly migrationsAssembly,
        TimeSpan commandTimeout,
        IServiceCollection services) : base(connectionString, migrationsAssembly, commandTimeout, services)
    {
    }

    protected override IServiceCollection GetMigratorServices(string connectionString, Assembly migrationsAssembly, TimeSpan commandTimeout) =>
        new ServiceCollection().AddPostgresMigrator(connectionString, migrationsAssembly, allowBreakingChange: false, commandTimeout);
}