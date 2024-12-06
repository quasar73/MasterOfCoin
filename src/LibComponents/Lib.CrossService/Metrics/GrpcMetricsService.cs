using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Lib.CrossService.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Lib.CrossService.Metrics;

internal sealed class GrpcMetricsService : EventSource
{
    public static readonly GrpcMetricsService Log = new();

    private TimeSpan _limit = TimeSpan.FromMilliseconds(300);

    private readonly ConcurrentDictionary<string, long> _amounts = new();
    private readonly ConcurrentDictionary<string, PollingCounter> _grpcCounters = new();

    internal GrpcMetricsService() : base("MetricsGrpc")
    {
    }

    //for testing
    internal GrpcMetricsService(string eventSourceName) : base(eventSourceName)
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [Event(eventId: 1, Level = EventLevel.Verbose)]
    public void SendMetrics(TimeSpan elapsed, string methodName, string targetHost, bool success)
    {
        var statusCounterKey = GetStatusCounterKey(methodName, targetHost, success);
        IncrementCounter(statusCounterKey, () => GetStatusDescription(methodName, targetHost, success));

        var timingCounterKey = GetTimingCounterKey(methodName, targetHost, elapsed, _limit);
        IncrementCounter(timingCounterKey, () => GetTimingDescription(methodName, targetHost, elapsed, _limit));

        WriteEvent(1, elapsed, methodName, targetHost, success);
    }

    [NonEvent]
    internal void SetRequestTimeMetricsLimit(TimeSpan limit)
    {
        _limit = limit;
    }

    [NonEvent]
    private void IncrementCounter(MetricCounterDetails details, Func<string> getDescriptionFunc)
    {
        var key = details.Name + ":" + string.Join(",", details.Tags?.Select(p => $"{p.Key}={p.Value}") ?? []);
        if (!_grpcCounters.TryGetValue(key, out var counter))
        {
            counter = new PollingCounter(details.Name, this, () => _amounts.TryGetValue(key, out var amount) ? amount : 0)
            {
                DisplayName = getDescriptionFunc(),
            };

            details.Tags?.ToList().ForEach(p => counter.AddMetadata(p.Key, p.Value));

            if (!_grpcCounters.TryAdd(key, counter))
            {
                counter.Dispose();
            }
        }

        WriteToCounter(key);
    }

    [NonEvent]
    private void WriteToCounter(string key)
    {
        _amounts.AddOrUpdate(key, 1, (_, oldValue) => oldValue + 1);
    }

    [NonEvent]
    private static string GetStatusDescription(string methodName, string targetHost, bool success)
    {
        var status = success ? "success" : "failed";
        return $"{status} grpc calls amount to the '{targetHost}.{methodName}'";
    }

    [NonEvent]
    private static string GetTimingDescription(string methodName, string targetHost, TimeSpan elapsed, TimeSpan limit)
    {
        var timing = elapsed <= limit ? "in-time" : "delayed";
        return $"{timing} grpc calls amount to the '{targetHost}.{methodName}'";
    }

    [NonEvent]
    private static MetricCounterDetails GetStatusCounterKey(string methodName, string targetHost, bool success)
    {
        var status = success ? "success" : "failed";
        return new($"{status}_total".ToLower(), new() { ["method"] = $"{targetHost}.{methodName}" });
    }

    [NonEvent]
    private static MetricCounterDetails GetTimingCounterKey(string methodName, string targetHost, TimeSpan elapsed, TimeSpan limit)
    {
        var timing = elapsed <= limit ? "within" : "outside";
        return new($"{timing}_limit_total".ToLower(), new() { ["method"] = $"{targetHost}.{methodName}" });
    }

    [NonEvent]
    protected override void Dispose(bool disposing)
    {
        _grpcCounters.Values.ToList().ForEach(counter => counter.Dispose());
        _grpcCounters.Clear();
        base.Dispose(disposing);
    }

    protected override void OnEventCommand(EventCommandEventArgs command)
    {
        if (command.Command == EventCommand.Enable)
        {
            // artificial counter to force the event source to work
            var _ = new PollingCounter("grpc_metrics_enabled_total", this, () => 1)
            {
                DisplayName = "grpc metrics enabled calls",
            };
        }
    }
}
