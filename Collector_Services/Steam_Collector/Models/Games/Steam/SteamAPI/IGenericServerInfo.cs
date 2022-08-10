using Okolni.Source.Query.Responses;
using UncoreMetrics.Data;

namespace UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;

public interface IGenericServerInfo
{
    public Server? ExistingServer { get; set; }

    public InfoResponse? ServerInfo { get; set; }

    public PlayerResponse? ServerPlayers { get; set; }


    public RuleResponse? ServerRules { get; set; }
}