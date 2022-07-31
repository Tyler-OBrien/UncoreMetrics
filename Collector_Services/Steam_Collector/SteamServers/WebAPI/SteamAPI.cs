using Microsoft.Extensions.Options;
using Steam_Collector.Models;

namespace Steam_Collector.SteamServers.WebAPI;

public partial class SteamAPI : ISteamAPI
{
    private readonly SteamCollectorConfiguration _steamCollectorConfiguration;
    private readonly HttpClient _httpClient;
    private readonly string? _STEAM_API_KEY;


    public SteamAPI(IOptions<SteamCollectorConfiguration> baseConfigurationOptions)
    {
        _httpClient = new HttpClient();
        _steamCollectorConfiguration = baseConfigurationOptions.Value;
        _httpClient.BaseAddress = new Uri("https://api.steampowered.com/");
        _STEAM_API_KEY = _steamCollectorConfiguration.SteamAPIKey;
    }
}