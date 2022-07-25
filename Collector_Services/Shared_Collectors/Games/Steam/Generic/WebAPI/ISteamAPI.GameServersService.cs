using Shared_Collectors.Models.Games.Steam.SteamAPI;

namespace Shared_Collectors.Games.Steam.Generic.WebAPI;

public partial interface ISteamAPI
{
    //Example:
    //https://api.steampowered.com/IGameServersService/GetServerList/v1/?key={key}&filter=\appid\1604030&limit=10
    public Task<List<SteamListServer>> GetServerList(string filter, int limit);

    //Example:
    //https://api.steampowered.com/IGameServersService/GetServerList/v1/?key={key}&filter=\appid\1604030&limit=10
    public Task<List<SteamListServer>> GetServerList(SteamServerListQueryBuilder filterBuilder, int limit);
}