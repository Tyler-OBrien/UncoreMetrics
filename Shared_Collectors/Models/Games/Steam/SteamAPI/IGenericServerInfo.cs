using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okolni.Source.Query.Responses;
using UncoreMetrics.Data;

namespace Shared_Collectors.Models.Games.Steam.SteamAPI
{
    public interface IGenericServerInfo<T> where T: GenericServer, new()
    {
        public T CustomServerInfo { get; set; }

        public InfoResponse? ServerInfo { get; set; }

        public PlayerResponse? ServerPlayers { get; set; }


        public RuleResponse? ServerRules { get; set; }
    }
}
