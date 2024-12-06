namespace Lib.CrossService.Models.Exceptions;

public class ErrorCodeException : Exception
{
    public int ErrorCode { get; set; }

    public ErrorCodeException(int errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
