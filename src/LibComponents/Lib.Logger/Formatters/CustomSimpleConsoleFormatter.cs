using System.Text;
using System.Text.Json;
using Lib.Logger.Extensions;
using Lib.Logger.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Lib.Logger.Formatters;

public sealed class CustomSimpleConsoleFormatter(IAsyncLoggerContext loggerContext) : ConsoleFormatter(nameof(CustomSimpleConsoleFormatter))
{
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var message = logEntry.Formatter.Invoke(logEntry.State, logEntry.Exception);
       
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        var builder = new StringBuilder()
            .AppendLine($"[{DateTimeOffset.UtcNow}] {GetLevelString(logEntry.LogLevel)} {message}")
            .AppendLine("\tPath = " + Utils.TryGuessCategory(logEntry.Category));

        if (logEntry.State is IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var contextValues = loggerContext.GetContextValues().ToDictionary(x => x.Key, x => x.Value as object);
            foreach (var kvp in parameters.Concat(contextValues))
            {
                if (kvp.Key != Utils.OriginalFormat)
                {
                    var key = kvp.Key;

                    if (Utils.Numbers.Contains(key))
                    {
                        key = "Param" + key;
                    }

                    var valueType = kvp.Value.GetType();
                    var value = valueType.IsUnsafeOrNull()
                        ? kvp.Value.ToString() ?? Utils.NullFormat
                        : JsonSerializer.Serialize(kvp.Value);

                    builder.AppendLine($"\t{key} = {value}");
                }
            }
        }

        textWriter.WriteLine(builder.ToString());
    }

    private static string GetLevelString(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Critical => "CRIT",
        LogLevel.Debug => "DBG ",
        LogLevel.Error => "ERR ",
        LogLevel.Information => "INFO",
        LogLevel.Trace => "TRC ",
        LogLevel.Warning => "WARN",
        _ => "NONE",
    };
}