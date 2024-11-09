using Microsoft.Extensions.Logging;

namespace Lib.Logger;

public class CustomLogger(ILoggerFactory loggerFactory) : ILogger
{
#pragma warning disable S3604 // Member initializer values should not be redundant
    private readonly ILogger _logger = loggerFactory.CreateLogger(Utils.TryGuessCategory(startTraceDepth: 2));
#pragma warning restore S3604 // Member initializer values should not be redundant

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        _logger.Log(logLevel, eventId, state, exception, formatter);
}