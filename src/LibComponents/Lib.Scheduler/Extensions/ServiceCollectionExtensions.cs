using Dapper;
using Hangfire;
using Hangfire.PostgreSql;
using Lib.Scheduler.Interfaces;
using Lib.Scheduler.Jobs;
using Lib.Scheduler.Options;
using Lib.Scheduler.Tracing;
using Lib.Scheduler.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Lib.Scheduler.Extensions;

public static class ServiceCollectionExtensions
{
    internal static ActivitySourceHolder? ActivitySourceHolder;

    public static IServiceCollection AddScheduler(this IServiceCollection services, IConfiguration configuration, string serviceName, string connectionString,
        int queuePollIntervalSec = 15, string? masterConnectionString = null)
    {
        if (ActivitySourceHolder != null)
        {
            throw new ApplicationException($"Duplicate call for {nameof(AddScheduler)} is not allowed");
        }

        ActivitySourceHolder = new ActivitySourceHolder(serviceName);

        services.Configure<SchedulerOptions>(configuration.GetSection(nameof(SchedulerOptions)));
        services.AddSingleton<ExpiredJobsCleaner>();

        return services
            .EnsureDatabase(connectionString, masterConnectionString)
            .AddSingleton<IScheduler, HangfireScheduler>()
            .AddSingleton(ActivitySourceHolder)
            .AddHangfireServer(options =>
            {
                options.SchedulePollingInterval = TimeSpan.FromSeconds(queuePollIntervalSec);
                options.Queues = [HangfireHelper.GetQueueName()];
            })
            .AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString), new PostgreSqlStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(queuePollIntervalSec)
                }));
    }

    private static IServiceCollection EnsureDatabase(this IServiceCollection services, string connectionString, string? masterConnectionString)
    {
        if (!string.IsNullOrWhiteSpace(masterConnectionString))
        {
            var dbConnection = new NpgsqlConnection(connectionString);
            var masterConnection = new NpgsqlConnection(masterConnectionString);

            var dbName = dbConnection.Database;
            var res = masterConnection.ExecuteScalar<int>($"SELECT count(*) FROM pg_database WHERE datname = '{dbName}';");

            if (res == 0)
            {
                masterConnection.Execute($"CREATE DATABASE {dbName};");
            }
        }

        return services;
    }
}
