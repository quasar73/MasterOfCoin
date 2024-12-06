using Grpc.Core;

namespace Lib.CrossService.Client
{
    public record ChannelInfo(CallInvoker Invoker, Uri TargetHostUri);
}
