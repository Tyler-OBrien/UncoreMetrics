using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Okolni.Source.Query.Responses;
using Shared_Collectors.Models.Tools.Maxmind;
using UncoreMetrics.Data;

namespace Shared_Collectors.Models.Games.Steam.SteamAPI
{
    public class PollServerInfo
    {
        public PollServerInfo(GenericServer server, InfoResponse? serverInfo,
            PlayerResponse? serverPlayers, RuleResponse? serverRules)
        {
      
            this.server = server;
            this.serverInfo = serverInfo;
            this.serverPlayers = serverPlayers;
            this.serverRules = serverRules;
        }



        public GenericServer server { get; set; }
        public InfoResponse? serverInfo { get; set; }

        public PlayerResponse? serverPlayers { get; set; }


        public RuleResponse? serverRules { get; set; }


        public GenericServer UpdateGenericServer(int nextCheckSeconds, List<int> nextCheckFailed, int daysUntilServerMarkedAsDead)
        {
            if (serverInfo == null)
            {
                server.FailedChecks += 1;
                var nextCheckFailedSeconds = nextCheckFailed.ElementAtOrDefault(server.FailedChecks - 1);
                if (nextCheckFailedSeconds == default(int))
                {
                    nextCheckFailedSeconds = nextCheckFailed.Last();
                }
                server.NextCheck = DateTime.UtcNow.AddSeconds(nextCheckFailedSeconds);
                if (server.FailedChecks > 1)
                {
                    server.Players = 0;
                    server.IsOnline = false;
                }
                if (server.LastCheck.AddDays(daysUntilServerMarkedAsDead) < DateTime.UtcNow)
                {
                    server.ServerDead = true;
                    server.NextCheck = DateTime.MaxValue;
                }
            }
            // Else if the check was successful
            else
            {
                server.NextCheck = DateTime.UtcNow.AddSeconds(nextCheckSeconds);
                server.FailedChecks = 0;
                server.Players = serverPlayers != null ? (uint)serverPlayers.Players.Count : serverInfo.Players;
                server.MaxPlayers = serverInfo.MaxPlayers;
                server.AppID = serverInfo.HasGameID ? (long)serverInfo.GameID : server.AppID;
                server.IsOnline = true;
                server.Game = serverInfo.Game;
            }

            return server;
        }

    }
}
