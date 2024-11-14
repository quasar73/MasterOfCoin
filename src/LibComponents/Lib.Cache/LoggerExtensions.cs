using Microsoft.Extensions.Logging;

namespace Lib.Cache;

public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Error,
        Message = "Failed to get connection from connection pool",
        EventName = nameof(ErrorGetConnectionFromPool),
        SkipEnabledCheck = true)]
    public static partial void ErrorGetConnectionFromPool(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Redis connection is in 'Disconnected' state",
        EventName = nameof(ErrorRedisDisconnected),
        SkipEnabledCheck = true)]
    public static partial void ErrorRedisDisconnected(this ILogger logger);
        
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to execute operation",
        EventName = nameof(ErrorOperationExecution),
        SkipEnabledCheck = true)]
    public static partial void ErrorOperationExecution(this ILogger logger, Exception ex);
}