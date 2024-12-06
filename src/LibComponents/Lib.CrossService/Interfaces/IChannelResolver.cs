using Lib.CrossService.Client;
using Microsoft.Extensions.Logging;

namespace Lib.CrossService.Interfaces
{
    public interface IChannelResolver
    {
        ChannelInfo ResolveChannel(string gateway, int defaultPort, Type contractType, bool useLoadBalancing, ILoggerFactory loggerFactory);
    }
}
