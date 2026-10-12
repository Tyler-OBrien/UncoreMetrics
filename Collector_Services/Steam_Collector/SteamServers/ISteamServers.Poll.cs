using UncoreMetrics.Data;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;

namespace UncoreMetrics.Steam_Collector.SteamServers;

public partial interface ISteamServers
{
    /// <summary>
    ///     Handles polling each of the input servers via Steam Server Query Protocol. Returns the result of the queries. 80
    ///     second timeout, so best to only pass in ~50k servers at a time.
    /// </summary>
    /// <param name="servers"></param>
    /// <returns>The list of resolved information about each server.</returns>
    /// <param name="queryRules">False to skip A2S_RULES (for games whose rules aren't used or that don't answer it)</param>
    public Task<List<PollServerInfo>> GenericServerPoll(List<Server> servers, CancellationToken token, bool queryRules = true);
}