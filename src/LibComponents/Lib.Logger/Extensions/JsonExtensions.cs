using System.Text.Json;

namespace Lib.Logger.Extensions;

public static class JsonExtensions
{
    public static string ToJson(this object? source, JsonSerializerOptions? options = null)
    {
        return source is null ? string.Empty : JsonSerializer.Serialize(source, options);
    }
}