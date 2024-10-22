using Lib.Db.Services;
using Lib.Db.Utils;
using Dapper;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Reflection;

namespace Lib.Db.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        string connectionString,
        Assembly? migrationsAssembly,
        string? masterConnectionString,
        bool allowBreakingChange = false,
        TimeSpan? commandTimeout = default)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        EnsureDatabase(connectionString, masterConnectionString);

        services
            .AddSingleton<IDataSourceHolder, DataSourceHolder>()
            .AddSingleton<IDatabaseFactory, PostgreDbFactory>()
            .AddTransient(sp =>sp.GetRequiredService<IDatabaseFactory>().Create(connectionString));

        if (migrationsAssembly != default)
        {
            services.AddPostgresMigrator(connectionString, migrationsAssembly, allowBreakingChange, commandTimeout);
        }

        return services;
    }

    public static IServiceCollection AddPostgresMigrator(this IServiceCollection services,
        string connectionString,
        Assembly migrationsAssembly,
        bool allowBreakingChange,
        TimeSpan? commandTimeout)
    {
        return services
            .AddFluentMigratorCore()
            .Configure<RunnerOptions>(opts => 
            {
                opts.Tags = [Constants.MigrationTagDatabase];
                opts.AllowBreakingChange = allowBreakingChange;
            })
            .ConfigureRunner(builder =>
            {
                builder.AddPostgres11_0()
                    .WithGlobalConnectionString(connectionString)
                    .WithMigrationsIn(migrationsAssembly)
                    .WithGlobalCommandTimeout(commandTimeout ?? TimeSpan.FromSeconds(30));
            })
            .AddLogging(builder => builder.AddFluentMigratorConsole());
    }

    private static void EnsureDatabase(string connectionString, string? masterConnectionString)
    {
        if (!string.IsNullOrWhiteSpace(masterConnectionString))
        {
            using var dbConnection = new NpgsqlConnection(connectionString);
            using var masterConnection = new NpgsqlConnection(masterConnectionString);

            var dbName = dbConnection.Database;
            var result = masterConnection.ExecuteScalar<int>($"SELECT count(*) FROM pg_database WHERE datname = '{dbName}';");

            if (result == 0)
            {
                masterConnection.Execute($"CREATE DATABASE {dbName};");
            }
        }
    }
}
