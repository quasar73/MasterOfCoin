namespace Lib.Db.Models;

public record MultipleResult<T1, T2>(
    List<T1> First, 
    List<T2> Second);