using Okolni.Source.Common;
using Okolni.Source.Query.Responses;
using UncoreMetrics.Data;
using UncoreMetrics.Steam_Collector.Helpers;

namespace UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;

public class PollServerInfo : IGenericServerInfo
{
    public PollServerInfo(Server existingServer, InfoResponse? serverInfo,
        PlayerResponse? serverPlayers, RuleResponse? serverRules)
    {
        ExistingServer = existingServer;
        ServerInfo = serverInfo;
        ServerPlayers = serverPlayers;
        ServerRules = serverRules;
        LastCheck = existingServer.LastCheck;
    }

    public DateTime LastCheck { get; set; }


    public Server ExistingServer { get; set; }


    public InfoResponse? ServerInfo { get; set; }

    public PlayerResponse? ServerPlayers { get; set; }


    public RuleResponse? ServerRules { get; set; }

    // If was down before, but up Now, or up but now down
    public bool StatusChanged { get; set; }


    internal void UpdateServer(ulong appid, int nextCheckSeconds, List<int> nextCheckFailed, int daysUntilServerMarkedAsDead)
    {
        if (ServerInfo == null)
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
            ExistingServer.Players = ServerInfo.Players;
            ExistingServer.MaxPlayers = ServerInfo.MaxPlayers;
            ExistingServer.AppID = ServerInfo.GameID ?? ExistingServer.AppID;
            ExistingServer.IsOnline = true;
            ExistingServer.Game = ServerInfo.Game;
            ExistingServer.Map = ServerInfo.Map;
            ExistingServer.Name = ServerInfo.Name;
            ExistingServer.LastCheck = DateTime.UtcNow;
            ExistingServer.Keywords = ServerInfo.KeyWords;
            ExistingServer.VAC = ServerInfo.VAC;
            ExistingServer.Visibility = ServerInfo.Visibility == Enums.Visibility.Private ? true : false;
            ExistingServer.Environment = ServerInfo.Environment.ResolveEnvironment();
            ExistingServer.SteamID = ServerInfo.SteamID;
        }
        // This server has switched SERVER GAME TYPES!!!!
        if (ExistingServer.AppID != 0 && ExistingServer.AppID != appid)
        {
            // Server is now dead to us.
            ExistingServer.ServerDead = true;
            ExistingServer.NextCheck = DateTime.MaxValue;
        }
    }
}