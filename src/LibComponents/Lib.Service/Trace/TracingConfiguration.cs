using Timer = System.Timers.Timer;

namespace Lib.Service.Trace;

public class TracingConfiguration(TraceSampler _sampler) : ITracingConfiguration
{
    private static readonly TimeSpan _defaultTracingDuration = TimeSpan.FromMinutes(5);

    private Timer? _timer;

    public void SetTracingEnabled(bool enabled, TimeSpan? duration = null)
    {
        if (enabled)
        {
            EnableTracing(duration);
        }
        else
        {
            DisableTracing();
        }
    }

    public bool IsTracingEnabled { get; private set; }

    private void EnableTracing(TimeSpan? duration)
    {
        _sampler.SetEnabled(true);
        IsTracingEnabled = true;

        var actualDuration = duration == null || duration == TimeSpan.Zero
            ? _defaultTracingDuration
            : duration.Value;

        _timer = new Timer(actualDuration);
        _timer.Elapsed += (_, _) => DisableTracing();
        _timer.Start();
    }

    private void DisableSampler()
    {
        _sampler.SetEnabled(false);
        IsTracingEnabled = false;
    }

    private void TryDisableTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
    }

    private void DisableTracing()
    {
        DisableSampler();
        TryDisableTimer();
    }
}