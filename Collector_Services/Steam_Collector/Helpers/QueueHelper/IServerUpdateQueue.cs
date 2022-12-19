using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncoreMetrics.Data.Discord;

namespace UncoreMetrics.Steam_Collector.Helpers.QueueHelper
{
    public interface IServerUpdateQueue
    {
        Task ServerUpdate(ServerUpdateNATs updateInfo);
    }
}
