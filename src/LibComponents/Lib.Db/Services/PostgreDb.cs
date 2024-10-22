using Lib.Db.Exceptions;
using Lib.Db.Models;
using Dapper;
using Npgsql;
using System.Data;

namespace Lib.Db.Services;

public class PostgreDb : IDatabase
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgreDb(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
        TryConnect();
    }

    public async Task DoInTransaction(Func<IDatabaseTransaction, Task> action)
    {
        await using var transaction = new PostgreDbTransaction(await _dataSource.OpenConnectionAsync());

        try
        {
            await action(transaction);
            await transaction.CommitInnerTransaction();
        }
        catch (RollbackException)
        {
            await transaction.RollbackInnerTransaction();
        }
    }

    public async Task<T?> DoInTransaction<T>(Func<IDatabaseTransaction, Task<T?>> action)
    {
        await using var transaction = new PostgreDbTransaction(await _dataSource.OpenConnectionAsync());
        T? result = default;

        try
        {
            result = await action(transaction);
            await transaction.CommitInnerTransaction();
        }
        catch (RollbackException)
        {
            await transaction.RollbackInnerTransaction();
        }

        return result;
    }

    public IAsyncEnumerable<T> GetStream<T>(string sql, object? parameters = null)
    {
        var connection = _dataSource.OpenConnection();
        return connection.QueryUnbufferedAsync<T>(sql, parameters);
    }

    public async Task<List<T>> GetList<T>(string sql, object? parameters = null)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        return (await connection.QueryAsync<T>(sql, parameters)).AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        return (await connection.QueryAsync(sql, map, parameters, default, true, splitOn)).AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        return (await connection.QueryAsync(sql, map, parameters, default, true, splitOn)).AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        return (await connection.QueryAsync(sql, map, parameters, default, true, splitOn)).AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        return (await connection.QueryAsync(sql, map, parameters, default, true, splitOn)).AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        return (await connection.QueryAsync(sql, map, parameters, default, true, splitOn)).AsList();
    }

    public async Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? parameters = null, string splitOn = "id")
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        return (await connection.QueryAsync(sql, map, parameters, default, true, splitOn)).AsList();
    }

    public async Task<T?> GetOrDefault<T>(string sql, object? parameters = null)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }

    public Task<int> Execute(string sql, object? parameters = null)
        => ExecuteInternal(sql, parameters, false);

    public async Task<MultipleResult<T1, T2>> GetMultiple<T1, T2>(string sql, object? parameters = null)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var reader = await connection.QueryMultipleAsync(sql, parameters);

        var first = (await reader.ReadAsync<T1>()).AsList();
        var second = (await reader.ReadAsync<T2>()).AsList();

        return new MultipleResult<T1, T2>(first, second);
    }

    public Task<int> ExecuteWithReloadTypes(string sql, object? parameters = null)
        => ExecuteInternal(sql, parameters, true);

    private async Task<int> ExecuteInternal(string sql, object? parameters, bool reloadTypes)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        var affected = await connection.ExecuteAsync(sql, parameters);

        if (reloadTypes)
        {
            ReloadTypes(connection);
        }

        return affected;
    }

    private void TryConnect()
    {
        using var connection = _dataSource.OpenConnection();
        ReloadTypes(connection);
    }

    private static void ReloadTypes(NpgsqlConnection connection)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }
        connection.ReloadTypes();
        connection.Close();
    }
}
