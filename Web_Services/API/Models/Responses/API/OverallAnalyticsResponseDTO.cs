using System.Text.Json.Serialization;

namespace UncoreMetrics.API.Models.Responses.API
{
    public class OverallAnalyticsResponseDTO
    {

        [JsonPropertyName("serverAnalyticsLast24H")]
        public ulong ServerAnalyticsLast24H { get; set; }

    }
}
