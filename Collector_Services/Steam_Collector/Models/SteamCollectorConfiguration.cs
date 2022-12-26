using UncoreMetrics.Data.Configuration;

namespace UncoreMetrics.Steam_Collector.Models;

public class SteamCollectorConfiguration : BaseConfiguration
{
    public string? SteamAPIKey { get; set; }

    public int ServersPerPollRun { get; set; }

    public int PollRunTimeout { get; set; }

    public int DiscoveryRunTimeout { get; set; }

    public int MaxConcurrency { get; set; }

    public int SecondsBetweenChecks { get; set; }

    public List<int> SecondsBetweenFailedChecks { get; set; }

    public int DaysUntilServerMarkedDead { get; set; }

    public string GameType { get; set; }

    public string NodeName { get; set; }

}