using System.Linq.Expressions;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace Lib.Scheduler.Interfaces;

public interface IScheduler
{
    string Enqueue(Expression<Func<Task>> methodCall);
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

    string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt);
    string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);

    void ScheduleOrUpdate(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression);
    void ScheduleOrUpdate<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression);

    void RemoveRecurringJobIfExists(string recurringJobId);
    List<RecurringJobDto> GetRecurringJobs();
    List<KeyValuePair<string, EnqueuedJobDto>> GetEnqueuedJobs();
    bool Delete(string jobId);
}
