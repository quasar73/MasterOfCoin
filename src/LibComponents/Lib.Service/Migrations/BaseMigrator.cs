using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using Lib.Service.Migrations.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TagsAttribute = Microsoft.AspNetCore.Http.TagsAttribute;

namespace Lib.Service.Migrations;

internal abstract class BaseMigrator : IMigrator
{
    private readonly IServiceProvider _serviceProvider;

    protected BaseMigrator(string connectionString,
        Assembly migrationsAssembly,
        TimeSpan commandTimeout,
        IServiceCollection services)
    {
        _serviceProvider = GetServiceProvider(connectionString, migrationsAssembly, commandTimeout, services);

        EnsureMigrationsTagsApplied(migrationsAssembly, services);
    }

    public virtual void MigrateUp()
    {
        using var scope = _serviceProvider.CreateScope();
        var runners = _serviceProvider.GetServices<IMigrationRunner>().ToArray();

        if (runners.Length == 0)
        {
            throw new InvalidOperationException("FluentMigrator runner is not registered.");
        }

        if (runners.Length > 1)
        {
            throw new InvalidOperationException("More than one FluentMigrator runner is registered.");
        }

        runners[0].MigrateUp();
    }

    protected abstract IServiceCollection GetMigratorServices(string connectionString, Assembly migrationsAssembly, TimeSpan commandTimeout);

    private IServiceProvider GetServiceProvider(string connectionString,
        Assembly migrationsAssembly,
        TimeSpan commandTimeout,
        IServiceCollection services)
    {
        var migratorServices = GetMigratorServices(connectionString, migrationsAssembly, commandTimeout);

        foreach (var service in services)
        {
            migratorServices.Add(service);
        }

        return migratorServices.BuildServiceProvider();
    }

    private static void EnsureMigrationsTagsApplied(Assembly migrationsAssembly, IServiceCollection services)
    {
        var migratorsCount = services.Count(x => x.ServiceType == typeof(IMigrator));
        var shouldEnsureMigrationsTags = migratorsCount > 1;
        if (shouldEnsureMigrationsTags)
        {
            var migrationTypes = migrationsAssembly.GetTypes()
                .Where(t => typeof(IMigration).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var migrationType in migrationTypes)
            {
                var tagsAttribute = migrationType.GetCustomAttribute<TagsAttribute>();
                if (tagsAttribute == null)
                {
                    throw new InvalidOperationException($"Migration {migrationType.Name} is missing the Tags attribute.");
                }
            }
        }
    }
}