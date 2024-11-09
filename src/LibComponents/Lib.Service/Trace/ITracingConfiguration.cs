namespace Lib.Service.Trace;

public interface ITracingConfiguration
{
    void SetTracingEnabled(bool enabled, TimeSpan? duration = null);
    bool IsTracingEnabled { get; }
}