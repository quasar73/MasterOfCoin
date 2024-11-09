using Lib.Logger.Formatters;
using Lib.Logger.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;


namespace Lib.Logger.Extensions;

public static class ServiceCollectionExtensions
{
    public static IWebHostBuilder AddLogging(this IWebHostBuilder webHostBuilder, bool simpleConsole = false, LogLevel minimumLevel = LogLevel.Trace)
    {
        return webHostBuilder
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder
                    .ClearProviders()
                    .SetMinimumLevel(minimumLevel);

                if (simpleConsole)
                {
                    loggingBuilder
                        .AddConsole(delegate (ConsoleLoggerOptions options)
                        {
                            options.FormatterName = "CustomSimpleConsoleFormatter";
                        })
                        .AddConsoleFormatter<CustomSimpleConsoleFormatter, ConsoleFormatterOptions>();
                }
                else
                {
                    loggingBuilder
                        .AddConsole(delegate (ConsoleLoggerOptions options)
                        {
                            options.FormatterName = "CustomJsonConsoleFormatter";
                        })
                        .AddConsoleFormatter<CustomJsonConsoleFormatter, ConsoleFormatterOptions>();
                }
            })
            .ConfigureServices(services =>
            {
                services.AddTransient<ILogger, CustomLogger>();
                services.AddSingleton<IAsyncLoggerContext, AsyncLoggerContext>();
            });
    }
}