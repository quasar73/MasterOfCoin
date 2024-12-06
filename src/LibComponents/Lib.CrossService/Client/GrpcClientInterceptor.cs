using AutoMapper;
using Lib.CrossService.Extensions;
using Lib.CrossService.Interfaces;
using Lib.CrossService.Metrics;
using Lib.CrossService.Models;
using Lib.CrossService.Tracing;
using Lib.CrossService.Utils;
using Lib.Logger.Extensions;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using GT = Google.Protobuf.WellKnownTypes;

namespace Lib.CrossService.Client
{
    public class GrpcClientInterceptor : AsyncInterceptorBase
    {
        private const string ResponseAsyncProperty = "ResponseAsync";

        private readonly Type _interfaceType;
        private readonly Type _clientType;
        private readonly object? _client;
        private readonly Uri _targetHost;
        private readonly ILogger<GrpcClientInterceptor> _logger;
        private readonly IDnsWrapper _dnsWrapper;
        private readonly ActivitySource? _clientActivitySource;

        private Dictionary<string, MethodInfo>? _grpcMethodCache;

        private IConfigurationProvider? _clientInConfig;
        private IConfigurationProvider? _clientOutConfig;

        public GrpcClientInterceptor(
            Type interfaceType,
            Type clientType,
            object? clientImplementation,
            Uri targetHost,
            Func<ActivitySourceHolder?> getSourceHolder,
            ILogger<GrpcClientInterceptor> logger,
            IDnsWrapper dnsWrapper)
        {
            _client = clientImplementation ?? throw new ArgumentNullException(nameof(clientImplementation));
            _interfaceType = interfaceType;
            _clientType = clientType;
            _logger = logger;
            _dnsWrapper = dnsWrapper;
            _targetHost = targetHost;
            _clientActivitySource = getSourceHolder
                .Invoke()?
                .GetActivitySource();

            InitReflection();
        }

		private void InitReflection()
        {
            _grpcMethodCache = _clientType.GetMethods()
                .ToDictionary(GrpcToKey);

            var inTypes = SourceTargetMap.GetDefaultStandartToGrpcMap();
            var outTypes = SourceTargetMap.GetDefaultGrpcToStandartMap();

            foreach (var method in _interfaceType.GetMethods())
            {
                var methodKey = method.ToAsyncKey();

                if (!_grpcMethodCache.TryGetValue(methodKey, out var grpcMethod))
                {
                    throw new KeyNotFoundException($"Can't find grpc method {methodKey}");
                }

                inTypes.AddRange(method.GetParameters()
                    .Select(p => p.ParameterType)
                    .Zip(grpcMethod.GetParameters()
                        .Where(p => !p.HasDefaultValue)
                        .Select(p => p.ParameterType))
                    .Select(SourceTargetMap.From));

                var returnType = method.ReturnType;
                var grpcReturnType = (grpcMethod.ReturnType.GetProperty(ResponseAsyncProperty)?.PropertyType)
                    ?? throw new InvalidCastException($"Can't find {ResponseAsyncProperty} property");

                if (returnType != typeof(Task))
                {
                    outTypes.Add(new(grpcReturnType, returnType));
                    outTypes.AddRange(grpcReturnType.GetGenericArguments()
                        .Zip(returnType.GetGenericArguments())
                        .Select(SourceTargetMap.From));
                }
            }

            _clientInConfig = inTypes.ToMapperConfiguration();
            _clientOutConfig = outTypes.ToMapperConfiguration();
        }

        protected override Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed) =>
            InterceptOrThrow<int>(invocation,
                async resultTask =>
                {
                    using var workActivity = _clientActivitySource?.StartActivity("Client.InterceptInner.Execute");
                    await resultTask;
                    return default;
                });

        protected override Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed) =>
            InterceptOrThrow(invocation,
                async resultTask =>
                {
                    var outMapper = _clientOutConfig!.CreateMapper();
                    using var workActivity = _clientActivitySource?.StartActivity("Client.InterceptInner.Execute");
                    await resultTask;
                    return outMapper.Map<Task<TResult>>(resultTask).Result;
                });

        public async Task<TResult> InterceptOrThrow<TResult>(IInvocation invocation, Func<Task, Task<TResult>> proceed)
        {
            using var activity = _clientActivitySource?.StartActivity("Client.Intercept");
            activity?.SetTag("interface.type", _interfaceType.FullName);
            activity?.SetTag("interface.method", invocation.Method);
            activity?.SetTag("client.type", _clientType.FullName);

            var success = false;
            var sw = GrpcMetricsService.Log.IsEnabled() ? Stopwatch.StartNew() : null;

            try
            {
                var result = await InterceptInner(invocation, proceed);
                success = true;

                return result;
            }
            catch (Exception exc)
            {
                activity?.SetStatus(ActivityStatusCode.Error, exc.Message);
                activity?.RecordException(exc);

                _logger.Error("Client interception failed", new
                {
                    InterfaceType = _interfaceType.FullName,
                    ServiceType = _clientType.FullName,
                    ServiceMethod = invocation.Method.Name,
                    InvokedMethodWithParams = $"{invocation.Method.Name}({JsonSerializer.Serialize(invocation.Arguments)})",
                    StartTime = activity?.GetTagItem(Constants.StartTimeMethodInvokeActivityTagKey),
                    ClientHost = Environment.MachineName,
                    ClientIP = _dnsWrapper.GetHostAddresses(null),
                    ServerHost = _targetHost,
                    ServerIPs = _dnsWrapper.GetHostAddresses(_targetHost.DnsSafeHost),
                    ElapsedTime = sw?.Elapsed,
                    ExceptionType = exc.GetType().Name,
                }, exc);

                throw;
            }
            finally
            {
                if (sw != null)
                {
                    sw.Stop();
                    GrpcMetricsService.Log.SendMetrics(sw.Elapsed, invocation.Method.Name, _targetHost.Host, success);
                }
            }
        }

        private Task<TResult> InterceptInner<TResult>(IInvocation invocation, Func<Task, Task<TResult>> proceed)
        {
            using var activity = _clientActivitySource?.StartActivity("Client.InterceptInner.Prepare");
            activity?.SetTag("client.method", invocation.Method.Name);

            var methodKey = invocation.Method.ToAsyncKey();

            if (!_grpcMethodCache!.TryGetValue(methodKey, out var grpcMethod))
            {
                throw new KeyNotFoundException($"Can't find grpc method {methodKey}");
            }

            var grpcParameters = grpcMethod.GetParameters()
                .ToList();

            var parameters = invocation.Method.GetParameters()
                .ToList();

            object?[] arguments;

            if (parameters.Any())
            {
                var inMapper = _clientInConfig!.CreateMapper();

                arguments = parameters
                    .Select((pt, i) => inMapper.Map(invocation.Arguments[i], pt.ParameterType, grpcParameters[i].ParameterType))
                    .Concat(grpcParameters
                        .Where(p => p.HasDefaultValue)
                        .Select(p => p.DefaultValue))
                    .ToArray();
            }
            else
            {
                arguments = grpcParameters
                    .Select(p => p.HasDefaultValue ? p.DefaultValue : new GT.Empty())
                    .ToArray();
            }

            activity?.Parent?.AddTag(Constants.StartTimeMethodInvokeActivityTagKey, DateTime.UtcNow);
            var asyncCallResult = grpcMethod.Invoke(_client, arguments);

            var taskResult = (asyncCallResult?.GetType().GetProperty(ResponseAsyncProperty))
                ?? throw new InvalidCastException($"Can't find {ResponseAsyncProperty} property");

            return proceed((Task)taskResult.GetValue(asyncCallResult)!);
        }

        private static string GrpcToKey(MethodInfo method)
        {
            var parametersCount = method.GetParameters()
                .Count(p => !p.HasDefaultValue && p.ParameterType != typeof(GT.Empty));

            return $"{method.Name}({parametersCount})";
        }
    }
}
