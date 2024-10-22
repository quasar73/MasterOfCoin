using System.Text;

namespace Lib.Db.Builders;

public abstract partial class BaseQuery<TQuery> where TQuery : BaseQuery<TQuery>
{
    protected readonly string _table;

    private int _limit;
    private long _offset;

    private readonly List<string> _selectColumns = [];
    private readonly List<string> _whereClauses = [];
    private readonly List<string> _orderByClauses = [];

    protected BaseQuery(string table)
    {
        _table = table;
    }

    public string Build()
    {
        var select = _selectColumns.Count > 0
            ? string.Join(", ", _selectColumns) 
            : "*";

        var result = new StringBuilder($"SELECT {select} FROM {_table}"); 

        if (_whereClauses.Count > 0)
        {
            result.Append(" WHERE ");
            result.Append(string.Join(" AND ", _whereClauses));
        }

        if (_orderByClauses.Count > 0)
        {
            result.Append(" ORDER BY ");
            result.Append(string.Join(", ", _orderByClauses));
        }

        if (_limit > 0)
        {
            result.Append($" LIMIT {_limit}");
        }

        if (_offset > 0)
        {
            result.Append($" OFFSET {_offset}");
        }

        return result.ToString();
    }

    public TQuery Limit(int limit)
    {
        _limit = limit;
        return (TQuery)this;
    }

    public TQuery Offset(int offset)
    {
        _offset = offset;
        return (TQuery)this;
    }

    public TQuery Offset(long offset)
    {
        _offset = offset;
        return (TQuery)this;
    }

    public TQuery OrderBy(params string[] columns)
    {
        foreach (var column in columns)
        {
            _orderByClauses.Add(column);
        }
        return (TQuery)this;
    }

    public TQuery OrderByDesc(params string[] columns)
    {
        foreach (var column in columns)
        {
            _orderByClauses.Add($"{column} DESC");
        }
        return (TQuery)this;
    }
}
