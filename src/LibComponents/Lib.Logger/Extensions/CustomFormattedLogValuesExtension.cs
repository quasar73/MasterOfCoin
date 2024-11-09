using System.Collections;

namespace Lib.Logger.Extensions;

public static class CustomFormattedLogValuesExtension
{
    private const int MaxDepthToPopulate = 10;
    private const int MaxItemsInCollectionPopulate = 10;

    public static void PopulateByObject(this IDictionary<string, object> dict, object? logParameters) =>
        dict.PopulateByObject(logParameters, null, 0);

    public static void PopulateByException(this IDictionary<string, object> dict, Exception? exception) =>
        dict.PopulateByException(exception, null);

    private static void PopulateByObject(this IDictionary<string, object> dict, object? logParameters, string? prefix, int depth)
    {
        if (logParameters == null)
        {
            return;
        }

        if (logParameters.GetType().IsUnsafeOrPrimitive())
        {
            dict.Add(prefix ?? "StringData", logParameters.ToString() ?? Utils.NullFormat);
        }
        else if (logParameters is Exception exception)
        {
            dict.PopulateByException(exception, prefix);
        }
        else if (logParameters is ICollection collection)
        {
            dict.PopulateByCollection(collection, prefix, depth);
        }
        else
        {
            dict.PopulateByProperties(logParameters, prefix, depth);
        }
    }

    private static void PopulateByCollection(this IDictionary<string, object> dict, ICollection collection, string? prefix, int depth)
    {
        dict.Add(GetNameWithPrefix(prefix, "Count"), collection.Count);

        var counter = 0;
        foreach (var item in collection)
        {
            if (counter >= MaxItemsInCollectionPopulate)
            {
                break;
            }

            dict.PopulateByObject(item, GetNameWithPrefix(prefix, $"Item{counter}"), depth + 1);
            counter++;
        }
    }

    private static void PopulateByProperties(this IDictionary<string, object> dict, object parameter, string? prefix, int depth)
    {
        if (depth >= MaxDepthToPopulate)
        {
            dict.Add(GetNameWithPrefix(prefix, "MaxDepthExceededException"), $"The max depth of {MaxDepthToPopulate} has been exceeded.");
            return;
        }

        var properties = parameter.GetType().GetProperties();

        foreach (var property in properties)
        {
            if (!property.CanRead)
            {
                continue;
            }

            if (property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            var value = property.GetValue(parameter) ?? Utils.NullFormat;
            dict.PopulateByObject(value, GetNameWithPrefix(prefix, property.Name), depth + 1);
        }
    }

    private static void PopulateByException(this IDictionary<string, object> dict, Exception? exception, string? prefix)
    {
        if (exception == null)
        {
            return;
        }

        dict[prefix + "ExceptionMessage"] = exception.Message;
        dict[prefix + "StackTrace"] = exception.StackTrace ?? Utils.NullFormat;

        dict.PopulateByException(exception.InnerException, prefix + "InnerException.");
    }

    private static string GetNameWithPrefix(string? prefix, string name) => (prefix is null ? string.Empty : prefix + ".") + name;
}