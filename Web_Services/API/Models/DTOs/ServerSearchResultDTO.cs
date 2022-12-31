using UncoreMetrics.Data;

namespace UncoreMetrics.API.Models.DTOs
{
    public class ServerSearchResultDTO
    {
        public Guid ServerID { get; set; }

        public string Name { get; set; }

        public string Game { get; set; }

        public string Map { get; set; }

        public ulong AppID { get; set; }

        public uint Players { get; set; }

        public uint MaxPlayers { get; set; }

        public string? ISP { get; set; }
        public string? Country { get; set; }


    }
}
