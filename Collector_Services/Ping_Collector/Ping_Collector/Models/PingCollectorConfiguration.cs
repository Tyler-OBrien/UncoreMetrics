using UncoreMetrics.Data.Configuration;

namespace Ping_Collector.Models
{
    public class PingCollectorConfiguration : BaseConfiguration
    {
        public int Prometheus_Metrics_Port { get; set; }

    }
}
