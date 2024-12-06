namespace Lib.CrossService.Interfaces;

public interface IObjectCache
{
    T GetOrCreate<T>();
}
