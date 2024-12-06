using Lib.CrossService.Interfaces;
using Lib.CrossService.Tracing;
using Castle.DynamicProxy;
using System.Diagnostics;
using System.Reflection;

namespace Lib.CrossService.Server
{
    internal class GrpcServiceSelector : IInterceptorSelector
    {
        private readonly HashSet<Type> _grpcServiceTypes;
        private readonly ActivitySource? _activitySource;

        public GrpcServiceSelector(IEnumerable<Type> grpcServiceTypes, ActivitySourceHolder activitySourceHolder)
        {
            _grpcServiceTypes = grpcServiceTypes.ToHashSet();
            _activitySource = activitySourceHolder.GetActivitySource();
        }

        public IEnumerable<Type> GrpcServiceTypes => _grpcServiceTypes;

        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            using var activity = _activitySource?.StartActivity("Server.SelectInterceptors");
            activity?.SetTag(nameof(type), type.FullName);
            activity?.SetTag(nameof(method), method.Name);

            if (!_grpcServiceTypes.Contains(type))
            {
                return Array.Empty<IInterceptor>();
            }

            activity?.AddEvent(new("Server grpc handler has found"));

            return interceptors
                .OfType<AsyncDeterminationInterceptor>()
                .Where(i => i.AsyncInterceptor is IGrpcServiceInterceptor grpcServiceInterceptor && grpcServiceInterceptor.Key == type.Name)
                .Cast<IInterceptor>()
                .ToArray();
        }
    }
}
