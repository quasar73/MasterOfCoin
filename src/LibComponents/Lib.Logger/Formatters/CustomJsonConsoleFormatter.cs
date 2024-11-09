using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lib.Logger.Extensions;
using Lib.Logger.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Lib.Logger.Formatters;

public sealed class CustomJsonConsoleFormatter(IAsyncLoggerContext loggerContext) : ConsoleFormatter(nameof(CustomJsonConsoleFormatter))
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { ReferenceHandler = ReferenceHandler.IgnoreCycles };

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
	{
		var message = logEntry.Formatter.Invoke(logEntry.State, logEntry.Exception);

		if (string.IsNullOrEmpty(message))
		{
			return;
		}

        var exceptionParameters = new Dictionary<string, object>();
        if (logEntry.State is not null
            && logEntry.State.GetType() != typeof(CustomFormattedLogValues))
        {
			exceptionParameters.PopulateByException(logEntry.Exception);
        }

        var jsonObject = new Dictionary<string, object>
		{
			["TimeStamp"] = DateTimeOffset.UtcNow,
			["LogLevel"] = logEntry.LogLevel.ToString(),
			["Message"] = message,
			["Path"] = Utils.TryGuessCategory(logEntry.Category),
			["HostName"] = Environment.MachineName,
			["ServiceName"] = Assembly.GetEntryAssembly()?.GetName().Name ?? Utils.NullFormat,
		};

        var stateParameters = logEntry.State as IEnumerable<KeyValuePair<string, object>> ?? new Dictionary<string, object>();
        var contextValues = loggerContext.GetContextValues().ToDictionary(x => x.Key, x => x.Value as object);
        var parameters = contextValues.Concat(stateParameters).Concat(exceptionParameters);

        foreach (var kvp in parameters)
        {
            if (kvp.Key != Utils.OriginalFormat)
            {
                var key = kvp.Key;

                if (Utils.Numbers.Contains(key))
                {
                    key = "Param" + key;
                }

                var valueType = kvp.Value?.GetType();

                jsonObject[key] = valueType.IsUnsafeOrNull()
                    ? kvp.Value?.ToString() ?? Utils.NullFormat
                    : kvp.Value!;
            }
        }

        // serialization with errors is discussed here: https://github.com/dotnet/runtime/issues/38049
        textWriter.WriteLine(JsonSerializer.Serialize(jsonObject, JsonSerializerOptions));
	}
}