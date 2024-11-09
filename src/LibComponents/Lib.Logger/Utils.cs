using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace Lib.Logger;

public static class Utils
{
    private const int MaxTraceDepth = 100;
    public const string OriginalFormat = "{OriginalFormat}";
    public const string NullFormat = "[null]";

    public static readonly ImmutableHashSet<string> Numbers = ImmutableHashSet.Create(Enumerable.Range(0, 100).Select(i => i.ToString()).ToArray());

    public static string TryGuessCategory(string? category = default, int startTraceDepth = 1)
    {
        if (!string.IsNullOrWhiteSpace(category))
        {
            return category;
        }

        var stack = new StackTrace();
        Type? parentType = null;
        var i = startTraceDepth;

        while (i < MaxTraceDepth)
        {
            var method = stack.GetFrame(i)?.GetMethod();
            if (TryInitParentType(method, ref parentType))
            {
                break;
            }

            i++;
        }

        return parentType?.FullName ?? NullFormat;
    }

    private static bool TryInitParentType(MethodBase? method, ref Type? parentType)
    {
        if (method == null)
        {
            return false;
        }

        if (method.IsConstructor)
        {
            parentType = method.DeclaringType;
            return true;
        }

        var reflectedType = method.ReflectedType;

        if (!IsNonSystemType(reflectedType))
        {
            return false;
        }

        if (!reflectedType!.IsAutoClass && reflectedType.DeclaringType == null)
        {
            parentType = reflectedType;
            return true;
        }

        if (reflectedType.DeclaringType == null)
        {
            return false;
        }

        parentType = reflectedType.DeclaringType;
        return true;
    }

    private static bool IsNonSystemType(Type? type)
    {
        return type != null
               && type != typeof(CustomLogger)
               && !string.IsNullOrWhiteSpace(type.FullName)
               && !type.FullName.StartsWith("System.")
               && !type.FullName.StartsWith("Microsoft.");
    }
}