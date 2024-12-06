namespace Lib.CrossService.Utils;

public static class Constants
{
    public const int InternalServerErrorCode = 500;
    public const string ErrorCodeHeaderKey = "xErrorCode";
    public const string ErrorMessageHeaderKey = "xErrorMessage";
    public const string StartTimeMethodInvokeActivityTagKey = "client.method.invoke.startTime";
}
