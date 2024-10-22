namespace Lib.Db.Utils;

/// <summary>
/// For test purposes
/// </summary>
public sealed class MultiLineStringComparer : IEqualityComparer<string>
{
    private static readonly char[] _ignoreChars = ['\r', '\n'];

    public bool Equals(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x == null || y == null)
            return false;

        var xLines = x.Split(_ignoreChars, StringSplitOptions.RemoveEmptyEntries).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim()).ToArray();
        var yLines = y.Split(_ignoreChars, StringSplitOptions.RemoveEmptyEntries).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim()).ToArray();

        var trimmedX = string.Join(" ", xLines);
        var trimmedY = string.Join(" ", yLines);

        return trimmedX == trimmedY;
    }

    public int GetHashCode(string obj)
    {
        throw new NotSupportedException();
    }
}
