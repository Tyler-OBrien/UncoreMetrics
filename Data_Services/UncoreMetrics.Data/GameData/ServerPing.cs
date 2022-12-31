using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UncoreMetrics.Data.GameData
{
    public class ServerPing
    {
        [ForeignKey("Server")]
        public Guid ServerId { get; set; }

        public int LocationID { get; set; }

        public float PingMs { get; set; }

        public bool Failed { get; set; }

        public DateTime LastCheck { get; set; }

        public DateTime NextCheck { get; set; }

        public int FailedChecks { get; set; }

        public void UpdateServerPing(int LocationId, int nextCheckSeconds, List<int> nextCheckFailed)
        {
            if (Failed)
            {
                FailedChecks += 1;
                var nextCheckFailedSeconds = nextCheckFailed.ElementAtOrDefault(FailedChecks - 1);
                if (nextCheckFailedSeconds == default) nextCheckFailedSeconds = nextCheckFailed.Last();
                NextCheck = DateTime.UtcNow.AddSeconds(nextCheckFailedSeconds);
            }
            // Else if the check was successful
            else
            {
                NextCheck = DateTime.UtcNow.AddSeconds(nextCheckSeconds);
            }
            LocationID = LocationId;
        }
    }

    public class Location
    {
        public int LocationID { get; set; }

        public string LocationName { get; set; }

        public string ISP { get; set; }

        public string ASN { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Country { get; set; }
    }
}
