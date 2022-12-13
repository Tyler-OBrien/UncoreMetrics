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
            get => ((double)OnlineCount / (double)PingCount) * (double)100;
        }

        public ulong PingCount { get; set; }

        public ulong OnlineCount { get; set; }

        public DateTime AverageTime { get; set; }
    }
    public class ClickHouseRawUptimeData
    {
        public ClickHouseRawUptimeData(Guid serverId, bool isOnline, DateTime checkTime)
        {
            ServerId = serverId;
            IsOnline = isOnline;
            CheckTime = checkTime;
        }

        public ClickHouseRawUptimeData()
        {

        }

        public Guid ServerId { get; set; }

        public bool IsOnline { get; set; }

        public DateTime CheckTime { get; set; }
    }
}
