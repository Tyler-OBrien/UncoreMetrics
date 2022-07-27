using Okolni.Source.Query.Responses;
using UncoreMetrics.Data;

namespace Steam_Collector.Models.Games.Steam.SteamAPI;

public class PollServerInfo : IGenericServerInfo
{
    public PollServerInfo(Server existingServer, InfoResponse? serverInfo,
        PlayerResponse? serverPlayers, RuleResponse? serverRules)
    {
        this.ExistingServer = existingServer;
        ServerInfo = serverInfo;
        ServerPlayers = serverPlayers;
        ServerRules = serverRules;
    }


    public Server ExistingServer { get; set; }


    public InfoResponse? ServerInfo { get; set; }

    public PlayerResponse? ServerPlayers { get; set; }


    public RuleResponse? ServerRules { get; set; }


    internal void UpdateServer(int nextCheckSeconds, List<int> nextCheckFailed, int daysUntilServerMarkedAsDead)
    {
        if (ServerInfo == null || String.IsNullOrWhiteSpace(ServerInfo.Folder))
        {
            ExistingServer.FailedChecks += 1;
            var nextCheckFailedSeconds = nextCheckFailed.ElementAtOrDefault(ExistingServer.FailedChecks - 1);
            if (nextCheckFailedSeconds == default) nextCheckFailedSeconds = nextCheckFailed.Last();
            ExistingServer.NextCheck = DateTime.UtcNow.AddSeconds(nextCheckFailedSeconds);
            if (ExistingServer.FailedChecks > 1)
            {
                ExistingServer.Players = 0;
                ExistingServer.IsOnline = false;
            }

            if (ExistingServer.LastCheck.AddDays(daysUntilServerMarkedAsDead) < DateTime.UtcNow)
            {
                ExistingServer.ServerDead = true;
                ExistingServer.NextCheck = DateTime.MaxValue;
            }
        }
        // Else if the check was successful
        else
        {
            ExistingServer.NextCheck = DateTime.UtcNow.AddSeconds(nextCheckSeconds);
            ExistingServer.FailedChecks = 0;
            ExistingServer.Players = ServerPlayers != null ? (uint)ServerPlayers.Players.Count : ServerInfo.Players;
            ExistingServer.MaxPlayers = ServerInfo.MaxPlayers;
            ExistingServer.AppID = ServerInfo.GameID ?? ExistingServer.AppID;
            ExistingServer.IsOnline = true;
            ExistingServer.Game = ServerInfo.Game;
            ExistingServer.Name = ServerInfo.Name;
        }
    }
}