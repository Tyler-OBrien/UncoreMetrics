using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Shared_Collectors.Games.Steam.Generic.WebAPI;
using Shared_Collectors.Models.Games.Steam.SteamAPI;

namespace Shared_Collectors.Games.Steam.Generic;

/// <summary>
///     Wrapper for Steam's Web API: <see href="https://partner.steamgames.com/doc/webapi/ISteamApps"></see>
/// </summary>
public partial class SteamAPI
{
    //Example:
    //https://api.steampowered.com/IGameServersService/GetServerList/v1/?key={key}&filter=\appid\1604030&limit=10
    public async Task<List<SteamListServer>> GetServerList(string filter, int limit)
    {
        var serverResponse = await _httpClient.GetFromJsonAsync<ServerListQueryResult>(
            $"IGameServersService/GetServerList/v1/?filter={Uri.EscapeDataString(filter)}&limit={limit}&key={_STEAM_API_KEY}");

        return serverResponse?.Response?.Servers ?? new List<SteamListServer>();
    }

    //Example:
    //https://api.steampowered.com/IGameServersService/GetServerList/v1/?key={key}&filter=\appid\1604030&limit=10
    public async Task<List<SteamListServer>> GetServerList(SteamServerListQueryBuilder filterBuilder, int limit)
    {
        var serverResponse = await _httpClient.GetFromJsonAsync<ServerListQueryResult>(
            $"IGameServersService/GetServerList/v1/?filter={filterBuilder}&limit={limit}&key={_STEAM_API_KEY}");

        return serverResponse?.Response?.Servers ?? new List<SteamListServer>();
    }

    //Example:
    //https://api.steampowered.com/IGameServersService/GetServerList/v1/?key={key}&filter=\appid\1604030&limit=10
    public async Task<List<SteamListServer>> GetServerListAppID(string AppID, int limit)
    {
        var newServerListBuilder = new SteamServerListQueryBuilder();

        newServerListBuilder.AppID(AppID);


        var tryRequest = await _httpClient.GetAsync(
            $"IGameServersService/GetServerList/v1/?filter={newServerListBuilder}&limit={limit}&key={_STEAM_API_KEY}");

        Console.WriteLine($"Request Uri: {tryRequest.RequestMessage.RequestUri}");

        var serverResponse = await tryRequest.Content.ReadFromJsonAsync<ServerListQueryResult>(new JsonSerializerOptions() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), Converters = { new SteamAPIInvalidUtf16Converter() }});

        return serverResponse?.Response?.Servers ?? new List<SteamListServer>();
    }
}