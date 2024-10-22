using Lib.Db.Exceptions;
using Lib.Db.Models;
using Dapper;
using Npgsql;

namespace Lib.Db.Services;

internal class PostgreDbTransaction : IDatabaseTransaction
{
    private bool _disposed;

    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;

    public PostgreDbTransaction(NpgsqlConnection connection)
    {
        _connection = connection;
        _transaction = _connection.BeginTransaction();
    }

    public IAsyncEnumerable<T> GetStream<T>(string sql, object? parameters = null) => _connection.QueryUnbufferedAsync<T>(sql, parameters, _transaction);

    public async Task<List<T>> GetList<T>(string sql, object? parameters = null)
    {
        return (await _connection
            .QueryAsync<T>(sql, parameters, _transaction))
            .AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        return (await _connection
            .QueryAsync(sql, map, parameters, _transaction, true, splitOn))
            .AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        return (await _connection
            .QueryAsync(sql, map, parameters, _transaction, true, splitOn))
            .AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        return (await _connection
            .QueryAsync(sql, map, parameters, _transaction, true, splitOn))
            .AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        return (await _connection
            .QueryAsync(sql, map, parameters, _transaction, true, splitOn))
            .AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        return (await _connection
            .QueryAsync(sql, map, parameters, _transaction, true, splitOn))
            .AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        return (await _connection
            .QueryAsync(sql, map, parameters, _transaction, true, splitOn))
            .AsList();
    }

    public async Task<T?> GetOrDefault<T>(string sql, object? parameters = null)
        => await _connection.QueryFirstOrDefaultAsync<T>(sql, parameters, _transaction);

    public Task<int> Execute(string sql, object? parameters = null)
        => _connection.ExecuteAsync(sql, parameters, _transaction);

    public async Task<MultipleResult<T1, T2>> GetMultiple<T1, T2>(string sql, object? parameters = null)
    {
        await using var reader = await _connection.QueryMultipleAsync(sql, parameters, _transaction);

        var first = (await reader.ReadAsync<T1>()).AsList();
        var second = (await reader.ReadAsync<T2>()).AsList();

        return new MultipleResult<T1, T2>(first, second);
    }

    public Task Rollback() => throw new RollbackException();

    internal Task CommitInnerTransaction() => _transaction.CommitAsync();
    internal Task RollbackInnerTransaction() => _transaction.RollbackAsync();

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                await _transaction.DisposeAsync();
                await _connection.DisposeAsync();
            }

            _disposed = true;
        }
    }
}
