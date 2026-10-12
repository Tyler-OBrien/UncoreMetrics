using Microsoft.AspNetCore.OutputCaching;

namespace UncoreMetrics.API.Services;

/// <summary>
///     Output cache policies for the expensive, shared read endpoints. Without them every page view on the website
///     reaches Postgres/ClickHouse (e.g. the home page's 11 per-game server counts at ~0.5 s each). Only successful GET
///     responses are cached, and each distinct route + query string is cached separately.
/// </summary>
public static class CachePolicies
{
    public const string Games = nameof(Games);
    public const string OverallAnalytics = nameof(OverallAnalytics);
    public const string ServerLists = nameof(ServerLists);
    public const string OverallPlayers = nameof(OverallPlayers);
    public const string Locations = nameof(Locations);
    public const string Jobs = nameof(Jobs);

    public static void Register(OutputCacheOptions options)
    {
        Add(options, Games, TimeSpan.FromHours(1));
        Add(options, OverallAnalytics, TimeSpan.FromMinutes(5));
        Add(options, ServerLists, TimeSpan.FromSeconds(30));
        Add(options, OverallPlayers, TimeSpan.FromMinutes(5));
        Add(options, Locations, TimeSpan.FromMinutes(10));
        // Collectors write job progress every 5 seconds
        Add(options, Jobs, TimeSpan.FromSeconds(5));
    }

    private static void Add(OutputCacheOptions options, string name, TimeSpan duration)
    {
        options.AddPolicy(name, policy => policy.Expire(duration).SetVaryByQuery("*"));
    }
}
