using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers.WebAPI;
using UncoreMetrics.Data;

namespace Steam_Collector.SteamServers;

public partial interface ISteamServers
{
    /// <summary>
    ///     Handles generic Steam Stats, grabbing active servers from the Steam Web API Server List, grabbing Server info /
    ///     rules / players, and returning results.
    /// </summary>
    /// <param name="appID"></param>
    /// <returns>Returns a list of full Server info to be actioned on with stats for that specific Server type</returns>
    public Task<List<DiscoveredServerInfo>> GenericServerDiscovery(SteamServerListQueryBuilder queryListQueryBuilder);
}