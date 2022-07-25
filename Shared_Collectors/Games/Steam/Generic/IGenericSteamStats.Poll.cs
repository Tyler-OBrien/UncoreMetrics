using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared_Collectors.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Data;

namespace Shared_Collectors.Games.Steam.Generic
{
    public partial interface IGenericSteamStats
    {
        public Task<List<PollServerInfo<T>>> GenericServerPoll<T>(ulong appID) where T: GenericServer, new();

    }
}
