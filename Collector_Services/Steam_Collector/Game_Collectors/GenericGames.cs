namespace UncoreMetrics.Steam_Collector.Game_Collectors;

/// <summary>
///     A game tracked by <see cref="GenericResolver" />: stored in the shared Servers table only, with no game-specific
///     table, so adding one needs no migration. Use a dedicated resolver and table instead when a game's A2S_RULES have
///     data worth keeping.
/// </summary>
/// <param name="Name">Game name stored on each server (and shown on the website)</param>
/// <param name="AppId">Steam app id, as servers register it on the Steam master server list</param>
/// <param name="PollBatchSize">Due servers to wait for before polling (see BaseResolver.ShouldPoll)</param>
public record GenericGame(string Name, ulong AppId, int PollBatchSize);

public static class GenericGames
{
    /// <summary>Keyed by the game type used in config / UNCORE_COLLECTOR_GAMETYPE.</summary>
    public static readonly IReadOnlyDictionary<string, GenericGame> All =
        new Dictionary<string, GenericGame>(StringComparer.OrdinalIgnoreCase)
        {
            ["CS2"] = new("Counter-Strike 2", 730, 1000),
            ["Valheim"] = new("Valheim", 892970, 1000),
            ["L4D2"] = new("Left 4 Dead 2", 550, 1000),
            ["GMod"] = new("Garry's Mod", 4000, 100),
            ["TF2"] = new("Team Fortress 2", 440, 100),
            ["Enshrouded"] = new("Enshrouded", 1203620, 100),
            ["Icarus"] = new("Icarus", 1149460, 100),
            ["SpaceEngineers"] = new("Space Engineers", 244850, 100),
        };
}
