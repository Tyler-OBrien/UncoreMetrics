using Shared_Collectors.Models.Games.Steam.SteamAPI;

namespace Shared_Collectors.Games.Steam.Generic;

public interface IGenericSteamStats
{
    /// <summary>
    ///     Handles generic Steam Stats, grabbing active servers from the Steam Web API Server List, grabbing Server info /
    ///     rules / players, and submitting to postgres & Clickhouse.
    /// </summary>
    /// <param name="appID"></param>
    /// <returns>Returns a list of full server info to be actioned on with stats for that specific server type</returns>
    public Task<List<FullServerInfo>> HandleGeneric(string appID);

    /// <summary>
    ///     Handles generic Steam Stats, grabbing Server info / rules / players, and submitting to Postgres & Clickhouse.
    /// </summary>
    /// <param name="servers"></param>
    /// <returns>Returns a list of full server info to be actioned on with stats for that specific server type</returns>
    public Task<List<FullServerInfo>> HandleGeneric(List<SteamListServer> steamListServers);
}