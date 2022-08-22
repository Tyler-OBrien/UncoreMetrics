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
        public ClickHouseUptimeData(Guid serverID, ulong appID, double uptime, DateTime averageTime)
        {
            ServerId = serverID;
            AppId = appID;
            Uptime = uptime;
            AverageTime = averageTime;
        }

        public Guid ServerId { get; set; }

        public ulong AppId { get; set; }

        public double Uptime { get; set; }
        public DateTime AverageTime { get; set; }
    }
}
