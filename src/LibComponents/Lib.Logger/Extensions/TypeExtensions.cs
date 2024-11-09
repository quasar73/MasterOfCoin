using System.Collections.Immutable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lib.Logger.Extensions;

internal static class TypeExtensions
{
    private static readonly ImmutableHashSet<string> UnsafeTypeNames = ["System.RuntimeType"];

    private static readonly ImmutableHashSet<Type> UnsafeTypes =
    [
        typeof(Endpoint),
        typeof(RouteEndpoint),
        typeof(Type),
        typeof(string),
        typeof(decimal),
        typeof(Guid),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
    ];

    public static bool IsUnsafeOrNull(this Type? type)
    {
        return type is null
               || type.IsUnsafe();
    }

    public static bool IsUnsafeOrPrimitive(this Type type)
    {
        return type.IsPrimitive
               || type.IsEnum
               || type.IsUnsafe();
    }

    private static bool IsUnsafe(this Type objType)
    {
        return UnsafeTypes.Contains(objType)
               || UnsafeTypeNames.Contains(objType.FullName ?? string.Empty);
    }
}