using UncoreMetrics.Data.Configuration;

namespace DiscordBot.Helpers
{
    public class UncoreDiscordBotConfiguration : BaseConfiguration
    {
        public string DiscordToken { get; set; }
        public int MaxServerLinksPerServer { get; set; }

        public int MaxServerLinksPerUser { get; set; }
    }
}