using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.VRising;

namespace Steam_Collector.Models
{
    public interface IGameResolver
    {
        public string Name { get;  }

        public Type ServerType { get; }

        public void ResolveCustomServerInfo<TServerType>(IGenericServerInfo<TServerType> server) where TServerType : Server, new();
    }
}
