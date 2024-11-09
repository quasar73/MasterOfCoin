using System.Diagnostics;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Lib.Scheduler.Interfaces;
using Lib.Scheduler.Tracing;
using Lib.Scheduler.Utils;

namespace Lib.Scheduler;

public class HangfireScheduler : IScheduler
    {
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IRecurringJobManager _recurringJobs;
        private readonly ActivitySource? _activitySource;
        private readonly string _queue = HangfireHelper.GetQueueName();

        public HangfireScheduler(IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobs, ActivitySourceHolder activitySourceHolder)
        {
            _backgroundJobs = backgroundJobs;
            _recurringJobs = recurringJobs;
            _activitySource = activitySourceHolder.ActivitySource;
        }

        public string Enqueue(Expression<Func<Task>> methodCall)
            => ExecuteWithTelemetry(nameof(Enqueue), () => _backgroundJobs.Create(methodCall, new EnqueuedState(_queue)));

        public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
            => ExecuteWithTelemetry(nameof(Enqueue), () => _backgroundJobs.Create(methodCall, new EnqueuedState(_queue)), ("service", typeof(T).FullName!));

        public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
            => ExecuteWithTelemetry(nameof(Schedule), () => _backgroundJobs.Schedule(_queue, methodCall, delay), ("delayTimeout", delay));

        public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
            => ExecuteWithTelemetry(nameof(Schedule), () => _backgroundJobs.Schedule(_queue, methodCall, delay), ("service", typeof(T).FullName!), ("delayTimeout", delay));

        public string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
            => ExecuteWithTelemetry(nameof(Schedule), () => _backgroundJobs.Schedule(_queue, methodCall, enqueueAt), ("executeAt", enqueueAt));

        public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
            => ExecuteWithTelemetry(nameof(Schedule), () => _backgroundJobs.Schedule(_queue, methodCall, enqueueAt), ("service", typeof(T).FullName!), ("executeAt", enqueueAt));

        public void ScheduleOrUpdate(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression)
        {
            ExecuteWithTelemetry(nameof(ScheduleOrUpdate), () =>
            {
                _recurringJobs.AddOrUpdate(recurringJobId, _queue, methodCall, cronExpression);
                return string.Empty;
            }, (nameof(cronExpression), cronExpression), (nameof(recurringJobId), recurringJobId));
        }

        public void ScheduleOrUpdate<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression)
        {
            ExecuteWithTelemetry(nameof(ScheduleOrUpdate), () =>
            {
                _recurringJobs.AddOrUpdate(recurringJobId, _queue, methodCall, cronExpression);
                return string.Empty;
            }, ("service", typeof(T).FullName!), (nameof(cronExpression), cronExpression), (nameof(recurringJobId), recurringJobId));
        }

        public void RemoveRecurringJobIfExists(string recurringJobId)
            => ExecuteWithTelemetry(nameof(RemoveRecurringJobIfExists), () =>
            {
                _recurringJobs.RemoveIfExists(recurringJobId);
                return string.Empty;
            }, (nameof(recurringJobId), recurringJobId));

        public List<RecurringJobDto> GetRecurringJobs()
        {
            using var connection = JobStorage.Current.GetConnection();
            return connection.GetRecurringJobs();
        }

        public List<KeyValuePair<string, EnqueuedJobDto>> GetEnqueuedJobs()
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var result = new List<KeyValuePair<string,EnqueuedJobDto>>();
            foreach (var queue in monitoringApi.Queues())
            {
                result.AddRange(monitoringApi.EnqueuedJobs(queue.Name, 0, int.MaxValue));
            }
            
            return result;
        }

        public bool Delete(string jobId) => _backgroundJobs.Delete(jobId);

        private string ExecuteWithTelemetry(string actionName, Func<string> action, params (string Key, object Value)[] tags)
        {
            using var activity = _activitySource?.StartActivity(actionName);
            if (tags != null)
            {
                foreach (var (key, value) in tags)
                {
                    activity?.AddTag(key, value);
                }
            }
            var result = action();
            if (!string.IsNullOrEmpty(result))
            {
                activity?.AddTag(nameof(result), result);
            }
            return result;
        }
    }
