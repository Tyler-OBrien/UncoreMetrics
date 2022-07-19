using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared_Collectors.Models.Games.Steam.SteamAPI;

namespace Shared_Collectors.Games.Steam.Generic
{
    public partial interface IGenericSteamStats
    {
        public Task<List<PollServerInfo>> GenericServerPoll(ulong appID);

    }
}
