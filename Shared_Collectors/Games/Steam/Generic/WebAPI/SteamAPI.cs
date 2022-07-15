namespace Shared_Collectors.Games.Steam.Generic;

public partial class SteamAPI
{
    private readonly HttpClient _httpClient;
    private readonly string _STEAM_API_KEY;


    public SteamAPI(string steamApiKey)
    {
        _httpClient = new HttpClient();

        _httpClient.BaseAddress = new Uri("https://api.steampowered.com/");
        _STEAM_API_KEY = steamApiKey;
    }
}