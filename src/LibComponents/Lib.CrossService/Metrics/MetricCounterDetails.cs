namespace Lib.CrossService.Metrics
{
    public record MetricCounterDetails(string Name, Dictionary<string, string> Tags);
}
