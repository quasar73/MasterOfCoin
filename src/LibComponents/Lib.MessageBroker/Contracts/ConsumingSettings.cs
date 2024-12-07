namespace Lib.MessageBroker.Contracts;

public class ConsumingSettings
{
    public ushort PrefetchCount { get; set; } = 1;
}
