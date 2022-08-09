using UncoreMetrics.Data.Configuration;

namespace Steam_Collector.Models;

public class SteamCollectorConfiguration : BaseConfiguration
{
    public string? SteamAPIKey { get; set; }

    public int SecondsBetweenChecks { get; set; }

    public List<int> SecondsBetweenFailedChecks { get; set; }

    public int DaysUntilServerMarkedDead { get; set; }

    public string GameType { get; set; }

    public string NodeName { get; set; }
}