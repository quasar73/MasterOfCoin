using System.Collections;
using Lib.Logger.Extensions;

namespace Lib.Logger.Formatters;

public class CustomFormattedLogValues : IEnumerable<KeyValuePair<string, object>>
{
    private readonly string _originalMessage;
    private readonly IDictionary<string, object> _parameters;

    public KeyValuePair<string, object> this[int index] =>
        index == Count - 1 ? new KeyValuePair<string, object>(Utils.OriginalFormat, _originalMessage) : _parameters.ElementAt(index);

    public int Count => _parameters.Count + 1;

    public CustomFormattedLogValues(string? message, object? args, Exception? exception)
    {
        _originalMessage = message ?? Utils.NullFormat;

        _parameters = new Dictionary<string, object>();
        _parameters.PopulateByObject(args);
        _parameters.PopulateByException(exception);
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        var i = 0;

        while (i < Count)
        {
            yield return this[i];

            var num = i + 1;
            i = num;
        }
    }

    public override string ToString()
    {
        return _originalMessage;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}