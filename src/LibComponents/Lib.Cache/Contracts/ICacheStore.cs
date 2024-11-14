using Lib.Cache.Enums;

namespace Base.Cache.Contracts;

public interface ICacheStore
{
    Task<string?> GetAsync(string key);
    Task<T?> GetAsync<T>(string key);

    Task<Dictionary<string, string?>> GetAsync(IEnumerable<string> keys);
    Task<Dictionary<string, T?>> GetAsync<T>(IEnumerable<string> keys);

    Task<bool> SetAsync(string key, string value, TimeSpan? ttl = default, When when = When.Always);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? ttl = default, When when = When.Always);

    Task<bool> SetAsync(Dictionary<string, string> keyValues);
    Task<bool> SetAsync<T>(Dictionary<string, T> keyValues);

    Task<string?> GetHashAsync(string hashName, string key);
    Task<T?> GetHashAsync<T>(string hashName, string key);

    Task<Dictionary<string, string?>> GetHashAsync(string hashName, IEnumerable<string> keys);
    Task<Dictionary<string, T?>> GetHashAsync<T>(string hashName, IEnumerable<string> keys);

    Task<bool> SetHashAsync(string hashName, string key, string value);
    Task<bool> SetHashAsync<T>(string hashName, string key, T value);

    Task SetHashAsync(string hashName, Dictionary<string, string> keyValues);
    Task SetHashAsync<T>(string hashName, Dictionary<string, T> keyValues);

    Task<bool> DeleteAsync(string key);
    Task<long> DeleteAsync(IEnumerable<string> keys);
    Task<bool> DeleteAsync(string hashName, string key);
    Task<long> DeleteAsync(string hashName, IEnumerable<string> keys);
}