using Lib.Db.Models;

namespace Lib.Db;

public interface IDatabaseBase
{
    /// <summary>
    /// Executes query and returns elements streamed data from database
    /// </summary>
    /// <typeparam name="T">Type of elements in resulting stream</typeparam>
    /// <param name="sql">Query string</param>
    /// <param name="parameters">Parameters object</param>
    IAsyncEnumerable<T> GetStream<T>(string sql, object? parameters = null);

    /// <summary>
    /// Executes query and returns elements collection from database
    /// </summary>
    /// <typeparam name="T">Type of elements in resulting collection</typeparam>
    /// <param name="sql">Query string</param>
    /// <param name="parameters">Parameters object</param>
    Task<List<T>> GetList<T>(string sql, object? parameters = null);

    /// <summary>
    /// Executes multi-mapping query with 2 input types and returns a single type, combined from the raw types via map
    /// </summary>
    /// <typeparam name="TReturn">Type of elements in resulting collection</typeparam>
    /// <param name="sql">Query string</param>
    /// <param name="map">The function to map row types to the return type</param>
    /// <param name="parameters">Parameters object</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "id")</param>
    Task<List<TReturn>> GetList<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? parameters = null, string splitOn = "id");
    
    /// <summary>
    /// Executes multi-mapping query with 2 input types and returns a single type, combined from the raw types via map
    /// </summary>
    /// <typeparam name="TReturn">Type of elements in resulting collection</typeparam>
    /// <param name="sql">Query string</param>
    /// <param name="map">The function to map row types to the return type</param>
    /// <param name="parameters">Parameters object</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "id")</param>
    Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? parameters = null, string splitOn = "id");
    
    /// <summary>
    /// Executes multi-mapping query with 2 input types and returns a single type, combined from the raw types via map
    /// </summary>
    /// <typeparam name="TReturn">Type of elements in resulting collection</typeparam>
    /// <param name="sql">Query string</param>
    /// <param name="map">The function to map row types to the return type</param>
    /// <param name="parameters">Parameters object</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "id")</param>
    Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? parameters = null, string splitOn = "id");
    
    /// <summary>
    /// Executes multi-mapping query with 2 input types and returns a single type, combined from the raw types via map
    /// </summary>
    /// <typeparam name="TReturn">Type of elements in resulting collection</typeparam>
    /// <param name="sql">Query string</param>
    /// <param name="map">The function to map row types to the return type</param>
    /// <param name="parameters">Parameters object</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "id")</param>
    Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? parameters = null, string splitOn = "id");
    
    /// <summary>
    /// Executes multi-mapping query with 2 input types and returns a single type, combined from the raw types via map
    /// </summary>
    /// <typeparam name="TReturn">Type of elements in resulting collection</typeparam>
    /// <param name="sql">Query string</param>
    /// <param name="map">The function to map row types to the return type</param>
    /// <param name="parameters">Parameters object</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "id")</param>
    Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? parameters = null, string splitOn = "id");
    
    /// <summary>
    /// Executes multi-mapping query with 2 input types and returns a single type, combined from the raw types via map
    /// </summary>
    /// <typeparam name="TReturn">Type of elements in resulting collection</typeparam>
    /// <param name="sql">Query string</param>
    /// <param name="map">The function to map row types to the return type</param>
    /// <param name="parameters">Parameters object</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "id")</param>
    Task<List<TReturn>> GetList<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? parameters = null, string splitOn = "id");

    /// <summary>
    /// Executes query and returns first element from database
    /// </summary>
    /// <typeparam name="T">Type of resulting element</typeparam>
    /// <param name="sql">Query string</param>
    /// <param name="parameters">Parameters object</param>
    Task<T?> GetOrDefault<T>(string sql, object? parameters = null);

    /// <summary>
    /// Just executes query without any feedback
    /// </summary>
    /// <param name="sql">Query string</param>
    /// <param name="parameters">Parameters object</param>
    /// <returns>The number of rows affected</returns>
    Task<int> Execute(string sql, object? parameters = null);

    /// <summary>
    /// Executes two queries in one round trip to the database
    /// </summary>
    /// <typeparam name="T1">Type of elements in first resulting collection</typeparam>
    /// <typeparam name="T2">Type of elements in second resulting collection</typeparam>
    /// <param name="sql">Query string</param>
    /// <param name="parameters">Parameters object</param>
    Task<MultipleResult<T1, T2>> GetMultiple<T1, T2>(string sql, object? parameters = null);
}