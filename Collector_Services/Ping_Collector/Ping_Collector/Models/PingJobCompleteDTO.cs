using UncoreMetrics.Data.GameData;

namespace Ping_Collector.Models
{
    public class PingJobCompleteDTO
    {
        public List<ServerPing> CompletedPings { get; set; }
    }
}
