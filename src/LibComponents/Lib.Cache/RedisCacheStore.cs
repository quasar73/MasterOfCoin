using System.Text.Json;
using Base.Cache.Contracts;
using Lib.Cache.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using When = Lib.Cache.Enums.When;

namespace Lib.Cache;

public class RedisCacheStore(IDatabaseProvider databaseProvider, ILogger<RedisCacheStore> logger) : ICacheStore
{
    public async Task<string?> GetAsync(string key) =>
        await SafeCall(db => db.StringGetAsync(key));

    public async Task<Dictionary<string, string?>> GetAsync(IEnumerable<string> keys)
    {
        var redisKeys = keys.ToRedisKeys();

        var values = await SafeCall(db => db.StringGetAsync(redisKeys));

        return values.ToDictionary(redisKeys) ?? new Dictionary<string, string?>();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await GetAsync(key);

        return string.IsNullOrEmpty(value)
            ? default
            : JsonSerializer.Deserialize<T>(value);
    }

    public async Task<Dictionary<string, T?>> GetAsync<T>(IEnumerable<string> keys)
    {
        var redisKeys = keys.ToRedisKeys();
        var values = await SafeCall(db => db.StringGetAsync(redisKeys));

        return values?.ToDictionary<T, RedisKey>(redisKeys) ?? new Dictionary<string, T?>();
    }

    public Task<bool> SetAsync(string key, string value, TimeSpan? ttl = default, When when = When.Always) =>
        SafeCall(db => db.StringSetAsync(key, value, ttl, when.ToRedis()));

    public Task<bool> SetAsync(Dictionary<string, string> keyValues) =>
        SafeCall(db => db.StringSetAsync(keyValues.ToRedisDictionary()));

    public Task<bool> SetAsync<T>(string key, T value, TimeSpan? ttl = default, When when = When.Always) =>
        SetAsync(key, JsonSerializer.Serialize(value), ttl, when);

    public Task<bool> SetAsync<T>(Dictionary<string, T> keyValues) =>
        SafeCall(db => db.StringSetAsync(keyValues.ToRedisDictionary()));

    public async Task<string?> GetHashAsync(string hashName, string key) =>
        await SafeCall(db => db.HashGetAsync(hashName, key));

    public async Task<Dictionary<string, string?>> GetHashAsync(string hashName, IEnumerable<string> keys)
    {
        var redisValues = keys.ToRedisValues();
        var values = await SafeCall(db => db.HashGetAsync(hashName, redisValues));

        return values.ToDictionary(redisValues) ?? new Dictionary<string, string?>();
    }

    public async Task<T?> GetHashAsync<T>(string hashName, string key)
    {
        var value = await GetHashAsync(hashName, key);

        return string.IsNullOrEmpty(value)
            ? default
            : JsonSerializer.Deserialize<T>(value);
    }

    public async Task<Dictionary<string, T?>> GetHashAsync<T>(string hashName, IEnumerable<string> keys)
    {
        var redisValues = keys.ToRedisValues();
        var values = await SafeCall(db => db.HashGetAsync(hashName, redisValues));

        return values?.ToDictionary<T, RedisValue>(redisValues) ?? new Dictionary<string, T?>();
    }

    public Task<bool> SetHashAsync(string hashName, string key, string value) =>
        SafeCall(db => db.HashSetAsync(hashName, key, value));

    public Task SetHashAsync(string hashName, Dictionary<string, string> keyValues) =>
        SafeCall(db => db.HashSetAsync(hashName, keyValues.ToHashEntries()));

    public Task<bool> SetHashAsync<T>(string hashName, string key, T value) =>
        SafeCall(db => db.HashSetAsync(hashName, key, JsonSerializer.Serialize(value)));

    public Task SetHashAsync<T>(string hashName, Dictionary<string, T> keyValues) =>
        SafeCall(db => db.HashSetAsync(hashName, keyValues.ToHashEntries()));

    public async Task<bool> DeleteAsync(string key)
        => await DeleteAsync(new List<string> { key }) > 0;

    public Task<long> DeleteAsync(IEnumerable<string> keys) =>
        SafeCall(db => db.KeyDeleteAsync(keys.ToRedisKeys()));

    public async Task<bool> DeleteAsync(string hashName, string key)
        => await DeleteAsync(hashName, new List<string> { key }) > 0;

    public Task<long> DeleteAsync(string hashName, IEnumerable<string> keys) =>
        SafeCall(db => db.HashDeleteAsync(hashName, keys.ToRedisValues()));

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private Task SafeCall(Func<IDatabase, Task> call) => SafeCall(async db =>
    {
        await call(db);
        return 0;
    });
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

    private async Task<T?> SafeCall<T>(Func<IDatabase, Task<T>> call)
    {
        var db = await databaseProvider.GetDatabase();
        if (db is null)
        {
            return default;
        }

        if (!db.Multiplexer.IsConnected)
        {
            logger.ErrorRedisDisconnected();
            return default;
        }

        try
        {
            return await call(db);
        }
        catch (Exception e)
        {
            logger.ErrorOperationExecution(e);
        }

        return default;
    }
}