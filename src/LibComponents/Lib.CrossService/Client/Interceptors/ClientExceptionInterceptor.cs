using Lib.CrossService.Models.Exceptions;
using Lib.CrossService.Utils;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Lib.CrossService.Client.Interceptors;

public class ClientExceptionInterceptor : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var call = continuation(request, context);

        var methodName = context.Method.FullName;

        return new AsyncUnaryCall<TResponse>(
            HandleResponse(call.ResponseAsync, call.ResponseHeadersAsync, methodName),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    private static async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> response, Task<Metadata> headers, string methodName)
    {
        var responseHeadersAsync = await headers;

        var entryErrCode = responseHeadersAsync.Get(Constants.ErrorCodeHeaderKey);
        var entryErrMessage = responseHeadersAsync.Get(Constants.ErrorMessageHeaderKey);

        if (entryErrCode != null || entryErrMessage != null)
        {
            var errCode = entryErrCode?.Value;
            var errMessage = entryErrMessage?.Value;
            var msg = $"{methodName}: {errMessage}";

            throw int.TryParse(errCode, out var number)
                ? new ErrorCodeException(number, msg)
                : new Exception(msg);
        }

        return await response;
    }
}
