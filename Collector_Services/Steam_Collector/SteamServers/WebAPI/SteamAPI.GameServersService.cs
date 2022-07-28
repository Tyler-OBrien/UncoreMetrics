using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Steam_Collector.Models.Games.Steam.SteamAPI;

namespace Steam_Collector.SteamServers.WebAPI;

/// <summary>
///     Wrapper for Steam's Web API: <see href="https://partner.steamgames.com/doc/webapi/ISteamApps"></see>
/// </summary>
public partial class SteamAPI
{
    public readonly JsonSerializerOptions options = new()
        { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), Converters = { new SteamAPIInvalidUtf16Converter() } };


    //Example:
    //https://api.steampowered.com/IGameServersService/GetServerList/v1/?key={key}&filter=\appid\1604030&limit=10
    public async Task<List<SteamListServer>> GetServerList(string filter, int limit)
    {
        var serverResponse = await _httpClient.GetFromJsonAsync<ServerListQueryResult>(
            $"IGameServersService/GetServerList/v1/?filter={Uri.EscapeDataString(filter)}&limit={limit}&key={_STEAM_API_KEY}",
            options);

        return serverResponse?.Response?.Servers ?? new List<SteamListServer>();
    }

    //Example:
    //https://api.steampowered.com/IGameServersService/GetServerList/v1/?key={key}&filter=\appid\1604030&limit=10
    /// <inheritdoc />
    public async Task<List<SteamListServer>> GetServerList(SteamServerListQueryBuilder filterBuilder, int limit)
    {
        var serverResponse = await _httpClient.GetFromJsonAsync<ServerListQueryResult>(
            $"IGameServersService/GetServerList/v1/?filter={filterBuilder}&limit={limit}&key={_STEAM_API_KEY}",
            options);

        return serverResponse?.Response?.Servers ?? new List<SteamListServer>();
    }
}