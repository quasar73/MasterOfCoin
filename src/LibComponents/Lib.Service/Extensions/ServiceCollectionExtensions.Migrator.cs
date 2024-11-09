using System.Globalization;
using System.Reflection;
using Lib.Service.Migrations;
using Lib.Service.Migrations.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.Service.Extensions;

public static partial class ServiceCollectionExtensions
{
    internal static IServiceCollection AddDatabaseMigrator(
        this IServiceCollection services,
        string connectionString,
        Assembly migrationsAssembly)
    {
        return services
            .AddTransient<IMigrator>(_ => new PostgresMigrator(
                connectionString,
                migrationsAssembly,
                GetMigratorCommandTimeout(),
                services));
    }

    private static TimeSpan GetMigratorCommandTimeout()
    {
        var shouldUseCustomCommandTimeout = TimeSpan.TryParse(Environment.GetEnvironmentVariable("MIGRATOR_COMMAND_TIMEOUT"), CultureInfo.InvariantCulture, out var dbCommandTimeout);
        var migratorCommandTimeout = shouldUseCustomCommandTimeout ? dbCommandTimeout : TimeSpan.FromSeconds(30);

        return migratorCommandTimeout;
    }
}