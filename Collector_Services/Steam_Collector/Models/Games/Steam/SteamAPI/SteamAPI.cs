using System.Text.Json.Serialization;

namespace Steam_Collector.Models.Games.Steam.SteamAPI;

public class ServerListQueryResult
{
    [JsonPropertyName("response")] public ServerListResponse Response { get; set; }
}

public class ServerListResponse
{
    [JsonPropertyName("servers")] public List<SteamListServer> Servers { get; set; }
}

public class SteamListServer
{
    /// <value>
    ///     Game Server Address, including port. Example: 100.64.192.29:27320. The port here is the query port, not the game
    ///     port.
    /// </value>
    [JsonPropertyName("addr")]
    public string Address { get; set; }


    /// <value>Game Server Port. Example: 140888. This differs from Address as the address is the query port.</value>
    [JsonPropertyName("gameport")]
    public long GamePort { get; set; }

    /// <value>Server Steam ID. Example: 90161059901175811</value>
    [JsonPropertyName("steamid")]
    public string SteamID { get; set; }

    /// <value>Server Name. Example: Valve CS:GO EU West Server (srcds119-fra2.271.306)</value>

    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <value>Application ID of the Server. Example: Valve CS:GO EU West Server (srcds119-fra2.271.306)</value>

    [JsonPropertyName("appid")]
    public ulong AppID { get; set; }

    /// <value>Game Directory / Game Name of the Server. Example: csgo</value>


    [JsonPropertyName("gamedir")]
    public string Gamedir { get; set; }

    /// <value>Game version the Server. Example: 1.38.3.6</value>

    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <value>Game product name of the Server. Example: csgo</value>


    [JsonPropertyName("product")]
    public string Product { get; set; }

    /// <value>
    ///     Some internal value for which region valve servers are running in. For servers hosted by anyone but valve, this
    ///     is just -1. Example: 3
    /// </value>


    [JsonPropertyName("region")]
    public long Region { get; set; }

    /// <value>Number of players in the Server. Example: 1</value>


    [JsonPropertyName("players")]
    public long Players { get; set; }

    /// <value>
    ///     Max number of players the Server is reporting. An important reminder: Custom servers could be lying about this,
    ///     nothing prevents it. Example: 10
    /// </value>


    [JsonPropertyName("max_players")]
    public long MaxPlayers { get; set; }

    /// <value>Number of bots in the Server. Example: 0</value>


    [JsonPropertyName("bots")]
    public long Bots { get; set; }

    /// <value>Map the Server is running. Example: de_dust2</value>


    [JsonPropertyName("map")]
    public string Map { get; set; }

    /// <value>If the Server is running any anticheat (may be VAC or something else like Battleye). Example: True</value>

    [JsonPropertyName("secure")]
    public bool Secure { get; set; }

    /// <value>If the Server is a dedicated Server. Example: True</value>


    [JsonPropertyName("dedicated")]
    public bool Dedicated { get; set; }

    /// <value>The OS the Server is running on. Example: L (Linux), W (Windows)</value>


    [JsonPropertyName("os")]
    public string Os { get; set; }

    /// <value>Comma seperated list of tags the Server has. Example: valve_ds,empty,secure</value>


    [JsonPropertyName("gametype")]
    public string Gametype { get; set; }
}