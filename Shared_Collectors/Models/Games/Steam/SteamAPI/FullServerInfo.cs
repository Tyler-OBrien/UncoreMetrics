using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okolni.Source.Query.Responses;

namespace Shared_Collectors.Models.Games.Steam.SteamAPI
{
    public class FullServerInfo
    {
        public FullServerInfo(SteamListServer? server, InfoResponse? serverInfo, PlayerResponse? serverPlayers, RuleResponse? serverRules)
        {
            this.server = server;
            this.serverInfo = serverInfo;
            this.serverPlayers = serverPlayers;
            this.serverRules = serverRules;
        }
        public SteamListServer? server { get; set; }
        public InfoResponse? serverInfo { get; set; }

        public PlayerResponse? serverPlayers { get; set; }


        public RuleResponse? serverRules { get; set; }

    }
}
