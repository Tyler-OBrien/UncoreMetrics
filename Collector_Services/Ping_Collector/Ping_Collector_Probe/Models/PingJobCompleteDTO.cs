using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncoreMetrics.Data.GameData;

namespace Ping_Collector_Probe.Models
{
    public class PingJobCompleteDTO
    {
        public List<ServerPing> CompletedPings { get; set; }
    }
}
