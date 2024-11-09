namespace Lib.Logger.Interfaces;

public interface IAsyncLoggerContext
{
    string? GetContextValue(string key);
    void SetContextValue(string key, string value);
    Dictionary<string, string> GetContextValues();
}