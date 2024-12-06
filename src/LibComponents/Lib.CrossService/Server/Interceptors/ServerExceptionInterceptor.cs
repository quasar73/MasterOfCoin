using Lib.CrossService.Interfaces;
using Lib.CrossService.Models.Exceptions;
using Lib.CrossService.Utils;
using Lib.Logger.Extensions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Lib.CrossService.Server.Interceptors;

public class ServerExceptionInterceptor(
    IObjectCache _objectCache,
    ILogger<ServerExceptionInterceptor> _logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception exc)
        {
            var actualException = (exc is TargetInvocationException && exc.InnerException != null)
                ? exc.InnerException
                : exc;

            if (actualException is not ErrorCodeException)
            {
                _logger.Error(actualException.Message, default, actualException);
            }

            var metadata = CreateErrorInfoHeaders(actualException);
            await context.WriteResponseHeadersAsync(metadata);

            return _objectCache.GetOrCreate<TResponse>();
        }
    }

    private static Metadata CreateErrorInfoHeaders(Exception exception)
    {
        var metadata = new Metadata()
        {
            { Constants.ErrorMessageHeaderKey, exception.ToString() },
        };

        var errorCode = (exception as ErrorCodeException)?.ErrorCode;

        if (errorCode.HasValue)
        {
            metadata.Add(Constants.ErrorCodeHeaderKey, errorCode.Value.ToString());
        }

        return metadata;
    }
}
