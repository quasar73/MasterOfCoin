using System.Collections.Concurrent;
using Lib.CrossService.Interfaces;

namespace Lib.CrossService.Server;

public class ObjectCache : IObjectCache
{
    private readonly ConcurrentDictionary<Type, object> _cache = new();

    public T GetOrCreate<T>()
    {
        var type = typeof(T);

        if (!_cache.TryGetValue(type, out var result))
        {
            var ctor = type.GetConstructor([]);
            result = ctor?.Invoke(default) ?? default!;

            _cache.TryAdd(type, result);
        }

        return (T)result;
    }
}
