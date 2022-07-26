using Okolni.Source.Query.Responses;
using UncoreMetrics.Data;

namespace Steam_Collector.Models.Games.Steam.SteamAPI;

public interface IGenericServerInfo<T> where T : Server, new()
{
    public T CustomServerInfo { get; set; }

    public InfoResponse? ServerInfo { get; set; }

    public PlayerResponse? ServerPlayers { get; set; }


    public RuleResponse? ServerRules { get; set; }
}