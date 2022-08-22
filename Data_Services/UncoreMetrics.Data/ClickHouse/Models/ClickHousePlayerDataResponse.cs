using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncoreMetrics.Data.ClickHouse.Models
{
    public class ClickHousePlayerData
    {
        public ClickHousePlayerData()
        {

        }
        public ClickHousePlayerData(Guid serverID, ulong appID, double playerAvg, uint playersMin, uint playersMax, DateTime averageTime)
        {
            ServerId = serverID;
            AppId = appID;
            PlayerAvg = playerAvg;
            PlayersMin = playersMin;
            PlayersMax = playersMax;
            AverageTime = averageTime;
        }
        public ClickHousePlayerData(DbDataReader reader)
        {
       
        }

        public Guid ServerId { get; set; }

        public ulong AppId { get; set; }

        public double PlayerAvg { get; set; }

        public uint PlayersMin { get; set; }
        public uint PlayersMax { get; set; }
        public DateTime AverageTime { get; set; }
    }
}
