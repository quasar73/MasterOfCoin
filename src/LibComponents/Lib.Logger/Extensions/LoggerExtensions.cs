using Lib.Logger.Formatters;
using Microsoft.Extensions.Logging;

namespace Lib.Logger.Extensions;

public static class LoggerExtensions
{
    public static void Critical(this ILogger logger, string message, object? args = null, Exception? exception = null)
    {
        logger.Log(LogLevel.Critical, default, new CustomFormattedLogValues(message, args, exception), exception, (_,_) => message);
    }

    public static void Debug(this ILogger logger, string message, object? args = null, Exception? exception = null)
    {
        logger.Log(LogLevel.Debug, default, new CustomFormattedLogValues(message, args, exception), exception, (_, _) => message);
    }

    public static void Error(this ILogger logger, string message, object? args = null, Exception? exception = null)
    {
        logger.Log(LogLevel.Error, default, new CustomFormattedLogValues(message, args, exception), exception, (_, _) => message);
    }

    public static void Info(this ILogger logger, string message, object? args = null, Exception? exception = null)
    {
        logger.Log(LogLevel.Information, default, new CustomFormattedLogValues(message, args, exception), exception, (_, _) => message);
    }

    public static void Trace(this ILogger logger, string message, object? args = null, Exception? exception = null)
    {
        logger.Log(LogLevel.Trace, default, new CustomFormattedLogValues(message, args, exception), exception, (_, _) => message);
    }

    public static void Warn(this ILogger logger, string message, object? args = null, Exception? exception = null)
    {
        logger.Log(LogLevel.Warning, default, new CustomFormattedLogValues(message, args, exception), exception, (_, _) => message);
    }
}