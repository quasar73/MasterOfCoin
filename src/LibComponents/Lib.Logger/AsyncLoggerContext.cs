using Lib.Logger.Interfaces;

namespace Lib.Logger;

public class AsyncLoggerContext : IAsyncLoggerContext
{
    private readonly AsyncLocal<Dictionary<string, string>> _sessionContext = new();

    public string? GetContextValue(string key) => _sessionContext.Value?.GetValueOrDefault(key);

    public Dictionary<string, string> GetContextValues() => _sessionContext.Value ?? new();

    public void SetContextValue(string key, string value)
    {
        _sessionContext.Value ??= new();
        _sessionContext.Value![key] = value;
    }
}