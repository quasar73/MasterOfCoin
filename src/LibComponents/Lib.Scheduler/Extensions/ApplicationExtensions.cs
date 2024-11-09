using Lib.Scheduler.Interfaces;
using Lib.Scheduler.Jobs;
using Lib.Scheduler.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lib.Scheduler.Extensions;

public static class ApplicationExtensions
{
    public static WebApplication AddCleanupSchedulerQueueJob(this WebApplication app)
    {
        var schedulerOptions = app.Services.GetService<IOptions<SchedulerOptions>>()?.Value;
        var scheduler = app.Services.GetService<IScheduler>();

        if (schedulerOptions?.QueuedJobsLifetime is null
            || schedulerOptions.QueuedJobsLifetime == TimeSpan.Zero
            || scheduler is null)
        {
            return app;
        }
        
        scheduler.Enqueue<ExpiredJobsCleaner>(cleaner => cleaner.CleanupQueuedJobs());
        
        return app;
    }
    
    public static IHost AddCleanupSchedulerQueueJob(this IHost host)
    {
        var schedulerOptions = host.Services.GetService<IOptions<SchedulerOptions>>()?.Value;
        var scheduler = host.Services.GetService<IScheduler>();

        if (schedulerOptions?.QueuedJobsLifetime is null
            || schedulerOptions.QueuedJobsLifetime == TimeSpan.Zero
            || scheduler is null)
        {
            return host;
        }

        scheduler.Enqueue<ExpiredJobsCleaner>(cleaner => cleaner.CleanupQueuedJobs());
        
        return host;
    }
}
