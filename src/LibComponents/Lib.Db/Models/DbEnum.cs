using Lib.Db.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Concurrent;

namespace Lib.Db.Models;

public readonly struct DbEnum<T> where T : Enum
{
    private static readonly ConcurrentDictionary<T, string> enumToStringCache = new();

    [JsonConverter(typeof(StringEnumConverter))]
    public T Value { get; }

    static DbEnum()
    {
        Dapper.SqlMapper.AddTypeHandler(typeof(DbEnum<T>), new DbEnumHandler<T>());
    }

    public DbEnum(T value)
    {
        Value = value;
    }

    public DbEnum(string name)
    {
        Value = Enum.TryParse(typeof(T), name, out var value)
            ? (T)value!
            : default!;
    }

    public override string ToString()
    {
        if (!enumToStringCache.TryGetValue(Value, out var result))
        {
            result = Value.ToString();
            enumToStringCache.TryAdd(Value, result);
        }

        return result;
    }

    public static implicit operator DbEnum<T>(T v) => new(v);
    public static implicit operator T(DbEnum<T> v) => v.Value;
    public static implicit operator DbEnum<T>(string s) => new(s);
}
