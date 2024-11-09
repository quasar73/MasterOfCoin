namespace Lib.Service.Settings;

public class ConnectionStrings
{
    public string? DbUri { get; set; }
    public bool DbEnableMultiplexing { get; set; } = false;
    public string? DbMasterUri { get; set; }
    public string? CacheUri { get; set; }
    public string? MessageBrokerUri { get; set; }
    public string? DistributedLockUri { get; set; }
    public string? TracingUri { get; set; }
    public string? ErrorTrackingUri { get; set; }
    public string? SchedulerUri { get; set; }
    public string? SchedulerMasterUri { get; set; }
    public string? GrpcGatewayUri { get; set; }
    public string? DataLakeUri { get; set; }
    public string? DataLakeMasterUri { get; set; }
}