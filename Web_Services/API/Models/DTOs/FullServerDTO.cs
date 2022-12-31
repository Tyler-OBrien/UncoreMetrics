using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData;

namespace UncoreMetrics.API.Models.DTOs
{
    public class FullServerDTO
    {

        public Guid ServerID { get; set; }

        public string Name { get; set; }

        public string Game { get; set; }

        public string Map { get; set; }

        public ulong AppID { get; set; }

        public string IpAddress { get; set; }

        public int Port { get; set; }

         public int QueryPort { get; set; }

         public uint Players { get; set; }

        public uint MaxPlayers { get; set; }

        public bool? Visibility { get; set; }



        public char? Environment { get; set; }


        public bool? VAC { get; set; }

        public string? Keywords { get; set; }

        public ulong? SteamID { get; set; }


        public long? ASN { get; set; }

        public string? ISP { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string? Country { get; set; }

        public string? Continent { get; set; }

        public string? Timezone { get; set; }

        public bool IsOnline { get; set; }

        public bool ServerDead { get; set; }


        public DateTime LastCheck { get; set; }

        public DateTime NextCheck { get; set; }

        public int FailedChecks { get; set; }

        public DateTime FoundAt { get; set; }

        public List<ServerPing> ServerPings { get; set; }

    }
}
