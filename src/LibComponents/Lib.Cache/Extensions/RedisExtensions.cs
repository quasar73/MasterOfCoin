using System.Text.Json;
using StackExchange.Redis;

namespace Lib.Cache.Extensions;

internal static class RedisExtensions
{
    internal static RedisKey[] ToRedisKeys(this IEnumerable<string> keys)
    {
            return keys
                .Select(k => new RedisKey(k))
                .ToArray();
        }

    internal static RedisValue[] ToRedisValues(this IEnumerable<string> keys)
    {
            return keys
                .Select(k => new RedisValue(k))
                .ToArray();
        }        
        
    internal static HashEntry[] ToHashEntries(this Dictionary<string, string> keyValues)
    {
            return keyValues
                .Select(kvp => new HashEntry(kvp.Key, kvp.Value))
                .ToArray();
        }

    internal static HashEntry[] ToHashEntries<T>(this Dictionary<string, T> keyValues)
    {
            return keyValues
                .Select(kvp => new HashEntry(kvp.Key, JsonSerializer.Serialize(kvp.Value)))
                .ToArray();
        }

    internal static KeyValuePair<RedisKey, RedisValue>[] ToRedisDictionary(this Dictionary<string, string> keyValues)
    {
            return keyValues
                .Select(kvp => new KeyValuePair<RedisKey, RedisValue>(kvp.Key, kvp.Value))
                .ToArray();
        }

    internal static KeyValuePair<RedisKey, RedisValue>[] ToRedisDictionary<T>(this Dictionary<string, T> keyValues)
    {
            return keyValues
                .Select(kvp => new KeyValuePair<RedisKey, RedisValue>(kvp.Key, JsonSerializer.Serialize(kvp.Value)))
                .ToArray();
        }

    internal static Dictionary<string, string?>? ToDictionary<TKey>(this RedisValue[]? values, TKey[] keys) where TKey : struct
    {
            return values?
                .Select((v, i) => new
                {
                    Key = keys[i].ToString()!,
                    Value = v.IsNull ? null : v.ToString()
                })
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

    internal static Dictionary<string, T?>? ToDictionary<T, TKey>(this RedisValue[] values, TKey[] keys) where TKey : struct
    {
            return values?
                .Select((v, i) => new
                {
                    Key = keys[i].ToString()!,
                    Value = string.IsNullOrEmpty(v) ? default : JsonSerializer.Deserialize<T>(v!)
                })
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

    internal static When ToRedis(this Enums.When when)
    {
            return when switch
            {
                Enums.When.Always => When.Always,
                Enums.When.NotExists => When.NotExists,
                Enums.When.Exists => When.Exists,
                _ => throw new ArgumentOutOfRangeException(nameof(when))
            };
        }
}