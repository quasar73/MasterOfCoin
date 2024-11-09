using System.Reflection;

namespace Lib.Scheduler.Utils;

internal static class HangfireHelper
{
    internal static string GetQueueName()
    {
        var appName = Assembly.GetCallingAssembly().GetName().Name;

        return $"{appName!.ToLower().Replace('.', '-')}";
    }
}
