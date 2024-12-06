using AutoMapper;
using Lib.CrossService.Extensions;
using Lib.CrossService.Interfaces;
using Lib.CrossService.Models;
using Lib.CrossService.Models.Exceptions;
using Lib.CrossService.Tracing;
using Lib.Logger.Extensions;
using Castle.DynamicProxy;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;
using GT = Google.Protobuf.WellKnownTypes;

namespace Lib.CrossService.Server
{
    public class GrpcServiceInterceptor : AsyncInterceptorBase, IGrpcServiceInterceptor
    {
        private readonly Type _grpcServiceType;
        private readonly Type _implementationType;
        private readonly object? _implementation;
        private readonly ILogger<GrpcServiceInterceptor> _logger;
        private readonly ActivitySource? _serviceActivitySource;

        private Dictionary<string, MethodInfo>? _implMethodCache;

        private IConfigurationProvider? _serviceInConfig;
        private IConfigurationProvider? _serviceOutConfig;

        public string Key => _grpcServiceType.Name;

        public GrpcServiceInterceptor(
            Type serviceType,
            Type interfaceType,
            object interfaceImplementation,
            Func<ActivitySourceHolder?> getSourceHolder,
            ILogger<GrpcServiceInterceptor> logger)
        {
            _grpcServiceType = serviceType;
            _implementationType = interfaceType;
            _implementation = interfaceImplementation;
            _logger = logger;
            _serviceActivitySource = getSourceHolder
                .Invoke()?
                .GetActivitySource();

            InitReflection();
        }

        private void InitReflection()
        {
            _implMethodCache = _implementationType.GetMethods()
                .ToDictionary(m => m.ToAsyncKey());

            var inTypes = SourceTargetMap.GetDefaultGrpcToStandartMap();
            var outTypes = SourceTargetMap.GetDefaultStandartToGrpcMap();

            var grpcMethods = _grpcServiceType.GetMethods()
                .Where(m => m.DeclaringType != typeof(object)); //to skip base methods

            foreach (var method in grpcMethods)
            {
                var methodKey = GrpcToKey(method);

                if (!_implMethodCache.TryGetValue(methodKey, out var implMethod))
                {
                    throw new KeyNotFoundException($"Can't find method {methodKey}");
                }

                inTypes.AddRange(method.GetParameters()
                    .Where(p => p.ParameterType != typeof(GT.Empty))
                    .Where(p => p.ParameterType != typeof(ServerCallContext))
                    .Select(p => p.ParameterType)
                    .Zip(implMethod.GetParameters()
                        .Select(p => p.ParameterType))
                    .Select(SourceTargetMap.From));

                var returnType = method.ReturnType;
                var implReturnType = implMethod.ReturnType;

                if (implReturnType != typeof(Task))
                {
                    outTypes.Add(new(implReturnType, returnType));
                    outTypes.AddRange(implReturnType.GetGenericArguments()
                        .Zip(returnType.GetGenericArguments())
                        .Select(SourceTargetMap.From));
                }
            }

            _serviceInConfig = inTypes.ToMapperConfiguration();
            _serviceOutConfig = outTypes.ToMapperConfiguration();
        }

        protected override Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed) =>
            InterceptOrThrow<int>(invocation,
                async resultTask =>
                {
                    using var workActivity = _serviceActivitySource?.StartActivity("Server.InterceptInner.Execute");
                    await resultTask;
                    return default;
                });

        protected override Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed) =>
            InterceptOrThrow(invocation,
                async resultTask =>
                {
                    var outMapper = _serviceOutConfig!.CreateMapper();
                    using var workActivity = _serviceActivitySource?.StartActivity("Server.InterceptInner.Execute");
                    await resultTask;
                    return outMapper.Map<Task<TResult>>(resultTask).Result;
                });

        public async Task<TResult> InterceptOrThrow<TResult>(IInvocation invocation, Func<Task, Task<TResult>> proceed)
        {
            using var activity = _serviceActivitySource?.StartActivity("Server.Intercept");
            activity?.SetTag("interface.type", _implementationType.FullName);
            activity?.SetTag("service.type", _grpcServiceType.FullName);
            activity?.SetTag("service.method", invocation.Method.Name);

            try
            {
                return await InterceptInner(invocation, proceed);
            }
            catch (Exception exc)
            {
                var actualException = (exc is TargetInvocationException && exc.InnerException != null)
                    ? exc.InnerException
                    : exc;

                if (actualException is not ErrorCodeException)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, actualException.Message);
                    activity?.RecordException(actualException);

                    _logger.Error("Server interception failed", new
                    {
                        InterfaceType = _implementationType.FullName,
                        ServiceType = _grpcServiceType.FullName,
                        ServiceMethod = invocation.Method.Name,
                    }, actualException);
                }

                throw;
            }
        }

        private Task<TResult> InterceptInner<TResult>(IInvocation invocation, Func<Task, Task<TResult>> proceed)
        {
            using var activity = _serviceActivitySource?.StartActivity("Server.InterceptInner.Prepare");
            activity?.SetTag("service.method", invocation.Method.Name);

            var methodKey = GrpcToKey(invocation.Method);

            if (!_implMethodCache!.TryGetValue(methodKey, out var implMethod))
            {
                throw new KeyNotFoundException($"Can't find implementation method {methodKey}");
            }

            var implParameters = implMethod.GetParameters()
                .ToList();

            var parameters = invocation.Method.GetParameters()
                .ToList();

            var inMapper = _serviceInConfig!.CreateMapper();

            var arguments = parameters
                .Where(p => p.ParameterType != typeof(GT.Empty))
                .Where(p => p.ParameterType != typeof(ServerCallContext))
                .Select((pt, i) => inMapper.Map(invocation.Arguments[i], pt.ParameterType, implParameters[i].ParameterType))
                .Concat(implParameters
                    .Where(p => p.HasDefaultValue)
                    .Select(p => p.DefaultValue))
                .ToArray();

            var taskResult = implMethod.Invoke(_implementation, arguments)
                ?? throw new Exception($"Invocation failed for method {methodKey}");

            return proceed((Task)taskResult);
        }

        private static string GrpcToKey(MethodInfo method)
        {
            var parametersCount = method.GetParameters()
                .Count(p => p.ParameterType != typeof(GT.Empty) && p.ParameterType != typeof(ServerCallContext));

            return $"{method.Name}Async({parametersCount})";
        }
    }
}
