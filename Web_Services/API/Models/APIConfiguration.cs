using UncoreMetrics.Data.Configuration;

namespace UncoreMetrics.API.Models;

public class APIConfiguration : BaseConfiguration
{
    public int Prometheus_Metrics_Port { get; set; }


}