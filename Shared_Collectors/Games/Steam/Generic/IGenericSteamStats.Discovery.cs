using Shared_Collectors.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Data;

namespace Shared_Collectors.Games.Steam.Generic;

public partial interface IGenericSteamStats
{
    /// <summary>
    ///     Handles generic Steam Stats, grabbing active servers from the Steam Web API Server List, grabbing Server info /
    ///     rules / players, and submitting to postgres & Clickhouse.
    /// </summary>
    /// <param name="appID"></param>
    /// <returns>Returns a list of full server info to be actioned on with stats for that specific server type</returns>
    public Task<List<DiscoveredServerInfo<T>>> GenericServerDiscovery<T>(ulong appID) where T : GenericServer, new();
}