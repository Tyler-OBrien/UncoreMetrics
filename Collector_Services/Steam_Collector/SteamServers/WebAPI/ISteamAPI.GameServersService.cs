using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;

namespace UncoreMetrics.Steam_Collector.SteamServers.WebAPI;

public partial interface ISteamAPI
{
    //Example:
    //https://api.steampowered.com/IGameServersService/GetServerList/v1/?key={key}&filter=\appid\1604030&limit=10
    /// <summary>
    ///     USes Steam Web API to grab the results of <paramref name="filter" />. The <paramref name="limit" /> is handled by
    ///     the Web API.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public Task<List<SteamListServer>> GetServerList(string filter, int limit);

    //Example:
    //https://api.steampowered.com/IGameServersService/GetServerList/v1/?key={key}&filter=\appid\1604030&limit=10
    /// <summary>
    ///     Uses Steam Web API to grab the results of <paramref name="filterBuilder" />. The <paramref name="limit" /> is
    ///     handled by the Web API.
    /// </summary>
    /// <param name="filterBuilder"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public Task<List<SteamListServer>> GetServerList(SteamServerListQueryBuilder filterBuilder, int imit);
}