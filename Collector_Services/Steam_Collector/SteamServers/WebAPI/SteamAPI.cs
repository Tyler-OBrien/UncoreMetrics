using Microsoft.Extensions.Options;
using UncoreMetrics.Steam_Collector.Models;

namespace UncoreMetrics.Steam_Collector.SteamServers.WebAPI;

public partial class SteamAPI : ISteamAPI
{
    private readonly HttpClient _httpClient;
    private readonly string? _STEAM_API_KEY;
    private readonly SteamCollectorConfiguration _steamCollectorConfiguration;


    public SteamAPI(IOptions<SteamCollectorConfiguration> baseConfigurationOptions)
    {
        _httpClient = new HttpClient();
        _steamCollectorConfiguration = baseConfigurationOptions.Value;
        _httpClient.BaseAddress = new Uri("https://api.steampowered.com/");
        _STEAM_API_KEY = _steamCollectorConfiguration.SteamAPIKey;
    }
}