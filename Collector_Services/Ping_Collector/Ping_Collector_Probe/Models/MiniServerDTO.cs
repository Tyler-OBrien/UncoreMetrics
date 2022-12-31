using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UncoreMetrics.Data.GameData;

namespace Ping_Collector_Probe.Models
{
    public class MiniServerDTO
    {
        public Guid ServerId { get; set; }
        [JsonIgnore]
        public IPAddress Address { get; set; }

        // IPAddress isn't seralizable...
        public string IpAddress { get; set; }

        public List<ServerPing> ServerPings { get; set; }
    }
}
