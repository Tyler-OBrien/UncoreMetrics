﻿using Microsoft.Extensions.Options;
using Steam_Collector.Models;

namespace Steam_Collector.SteamServers.WebAPI;

public partial class SteamAPI : ISteamAPI
{
    private readonly BaseConfiguration _baseConfiguration;
    private readonly HttpClient _httpClient;
    private readonly string? _STEAM_API_KEY;


    public SteamAPI(IOptions<BaseConfiguration> baseConfigurationOptions)
    {
        _httpClient = new HttpClient();
        _baseConfiguration = baseConfigurationOptions.Value;
        _httpClient.BaseAddress = new Uri("https://api.steampowered.com/");
        _STEAM_API_KEY = _baseConfiguration.SteamAPIKey;
    }
}