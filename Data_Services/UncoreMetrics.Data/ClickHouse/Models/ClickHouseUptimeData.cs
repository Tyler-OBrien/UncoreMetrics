using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncoreMetrics.Data.ClickHouse.Models
{
    public class ClickHouseUptimeData
    {
        public ClickHouseUptimeData()
        {

        }
        public ClickHouseUptimeData(Guid serverID, ulong appID, ulong pingCount, ulong onlineCount, DateTime averageTime)
        {
            ServerId = serverID;
            AppId = appID;
            PingCount = pingCount;
            OnlineCount = onlineCount;
            AverageTime = averageTime;
        }

        public Guid ServerId { get; set; }

        public ulong AppId { get; set; }

        public double Uptime
        {
            get => (OnlineCount / PingCount) * 100;
        }

        public ulong PingCount { get; set; }

        public ulong OnlineCount { get; set; }

        public DateTime AverageTime { get; set; }
    }
}
