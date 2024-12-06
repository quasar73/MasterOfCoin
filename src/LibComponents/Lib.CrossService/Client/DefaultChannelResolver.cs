using Lib.CrossService.Client.Interceptors;
using Lib.CrossService.Interfaces;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Logging;

namespace Lib.CrossService.Client
{
    public class DefaultChannelResolver : IChannelResolver
    {
        private readonly bool _mapToGatewayRoot;

        public DefaultChannelResolver(bool mapToGatewayRoot)
        {
            _mapToGatewayRoot = mapToGatewayRoot;
        }

        public ChannelInfo ResolveChannel(string gateway, int defaultPort, Type contractType, bool useLoadBalancing, ILoggerFactory loggerFactory)
        {
            var target = _mapToGatewayRoot ? gateway : $"{contractType.FullName!.ToLowerInvariant()}.{gateway}";

            var channelOptions = useLoadBalancing
                ? new GrpcChannelOptions
                {
                    ServiceConfig = new() { LoadBalancingConfigs = { new RoundRobinConfig() }},
                    Credentials = ChannelCredentials.Insecure,
                    LoggerFactory = loggerFactory,
                } 
                : new GrpcChannelOptions();

            var targetParts = target.Split(":");
            var port = targetParts.Length == 2 ? int.Parse(targetParts[1]) : defaultPort;

            var uri = new UriBuilder(Uri.UriSchemeHttp, targetParts[0], port);
            
            var channel = GrpcChannel.ForAddress(uri.ToString(), channelOptions);
            var invoker = channel.Intercept(new ClientExceptionInterceptor());

            return new(invoker, uri.Uri);
        }
    }
}