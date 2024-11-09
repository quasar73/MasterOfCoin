using Lib.Scheduler.Interfaces;
using Lib.Scheduler.Options;
using Microsoft.Extensions.Options;

namespace Lib.Scheduler.Jobs;

public class ExpiredJobsCleaner(IScheduler _scheduler, IOptions<SchedulerOptions> _options)
{
    public Task CleanupQueuedJobs()
    {
        if (_options.Value.QueuedJobsLifetime is null || _options.Value.QueuedJobsLifetime == TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }
        
        var expirationPeriod = _options.Value.QueuedJobsLifetime!.Value;
        
        //removing recurring jobs
        foreach (var job in _scheduler.GetRecurringJobs())
        {
            var lastExecution = job.LastExecution ?? job.CreatedAt;
            if (lastExecution < DateTime.UtcNow.Subtract(expirationPeriod))
            {
                _scheduler.RemoveRecurringJobIfExists(job.Id);
            }
        }
        
        //trying to mark enqueued jobs as deleted
        foreach (var job in _scheduler.GetEnqueuedJobs())
        {
            if (job.Value.EnqueuedAt < DateTime.UtcNow.Subtract(expirationPeriod))
            {
                _scheduler.Delete(job.Key);
            }
        }

        return Task.CompletedTask;
    }
}
