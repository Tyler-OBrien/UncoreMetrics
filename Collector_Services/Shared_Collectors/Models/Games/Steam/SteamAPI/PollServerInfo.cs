using Okolni.Source.Query.Responses;
using UncoreMetrics.Data;

namespace Shared_Collectors.Models.Games.Steam.SteamAPI;

public class PollServerInfo<T> : IGenericServerInfo<T> where T : Server, new()
{
    public PollServerInfo(Server server, InfoResponse? serverInfo,
        PlayerResponse? serverPlayers, RuleResponse? serverRules)
    {
        this.server = server;
        ServerInfo = serverInfo;
        ServerPlayers = serverPlayers;
        ServerRules = serverRules;
    }


    public Server server { get; set; }

    public T CustomServerInfo { get; set; }

    public InfoResponse? ServerInfo { get; set; }

    public PlayerResponse? ServerPlayers { get; set; }


    public RuleResponse? ServerRules { get; set; }


    internal T CreateCustomServerInfo(int nextCheckSeconds, List<int> nextCheckFailed, int daysUntilServerMarkedAsDead)
    {
        CustomServerInfo = new T();
        CustomServerInfo.Copy(server);
        if (ServerInfo == null)
        {
            CustomServerInfo.FailedChecks += 1;
            var nextCheckFailedSeconds = nextCheckFailed.ElementAtOrDefault(server.FailedChecks - 1);
            if (nextCheckFailedSeconds == default) nextCheckFailedSeconds = nextCheckFailed.Last();
            CustomServerInfo.NextCheck = DateTime.UtcNow.AddSeconds(nextCheckFailedSeconds);
            if (CustomServerInfo.FailedChecks > 1)
            {
                CustomServerInfo.Players = 0;
                CustomServerInfo.IsOnline = false;
            }

            if (CustomServerInfo.LastCheck.AddDays(daysUntilServerMarkedAsDead) < DateTime.UtcNow)
            {
                CustomServerInfo.ServerDead = true;
                CustomServerInfo.NextCheck = DateTime.MaxValue;
            }
        }
        // Else if the check was successful
        else
        {
            CustomServerInfo.NextCheck = DateTime.UtcNow.AddSeconds(nextCheckSeconds);
            CustomServerInfo.FailedChecks = 0;
            CustomServerInfo.Players = ServerPlayers != null ? (uint)ServerPlayers.Players.Count : ServerInfo.Players;
            CustomServerInfo.MaxPlayers = ServerInfo.MaxPlayers;
            CustomServerInfo.AppID = ServerInfo.GameID ?? server.AppID;
            CustomServerInfo.IsOnline = true;
            CustomServerInfo.Game = ServerInfo.Game;
            CustomServerInfo.Name = ServerInfo.Name;
        }

        return CustomServerInfo;
    }
}