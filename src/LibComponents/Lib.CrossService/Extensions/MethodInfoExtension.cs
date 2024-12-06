using System.Reflection;

namespace Lib.CrossService.Extensions
{
    internal static class MethodInfoExtension
    {
        private const string AsyncSuffix = "Async";
        private static Dictionary<string, string> _asyncKeyCache = new();

        public static string ToAsyncKey(this MethodInfo method)
        {
            ArgumentNullException.ThrowIfNull(method);

            if (!_asyncKeyCache.TryGetValue(method.Name, out var key))
            {
                key = method.Name.EndsWith(AsyncSuffix)
                    ? method.Name
                    : method.Name + AsyncSuffix;

                _asyncKeyCache[method.Name] = key;
            }

            return $"{key}({method.GetParameters().Count()})";
        }
    }
}
