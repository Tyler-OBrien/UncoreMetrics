using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using UncoreMetrics.Data.GameData;

namespace Ping_Collector.Models
{
    public class MiniServerDTO
    {
        public Guid ServerId { get; set; }
        [JsonIgnore]
        public IPAddress Address { get; set; }

        // IPAddress isn't seralizable...
        public string IpAddress => Address.ToString();

        public List<ServerPing> ServerPings { get; set; }
    }
}
