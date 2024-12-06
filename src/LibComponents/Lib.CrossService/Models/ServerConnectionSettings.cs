namespace Lib.CrossService.Models
{
    public class ServerConnectionSettings
    {
        public bool MapToGatewayRoot { get; set; }
        public bool UseLoadBalancing { get; set; }
        public TimeSpan? RequestTimeMetricsLimit { get; set; }
    }
}
