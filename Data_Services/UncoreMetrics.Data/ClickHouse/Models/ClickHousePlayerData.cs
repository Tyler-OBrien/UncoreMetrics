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

        public Guid ServerId { get; set; }

        public ulong AppId { get; set; }

        public double PlayerAvg { get; set; }

        public uint PlayersMin { get; set; }
        public uint PlayersMax { get; set; }
        public DateTime AverageTime { get; set; }
    }

    public class ClickHouseRawPlayerData
    {
        public ClickHouseRawPlayerData()
        {

        }
        public ClickHouseRawPlayerData(Guid serverID, uint players, DateTime checkTime)
        {
            ServerId = serverID;
            Players = players;
            CheckTime = checkTime;
        }

        public Guid ServerId { get; set; }


        public uint Players { get; set; }

        public DateTime CheckTime { get; set; }
    }
}
