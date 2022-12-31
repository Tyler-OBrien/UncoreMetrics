using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncoreMetrics.Data.GameData;

namespace Ping_Collector_Probe.Models
{
    public class ProbeConfiguration
    {
        public string SENTRY_DSN { get; set; }

        public string API_ENDPOINT { get; set; }
        public Dictionary<string, string> CUSTOM_REQUEST_HEADERS_FOR_API { get; set; }

        public int PollRunTimeout { get; set; }

        public int MaxConcurrency { get; set; }

        public int SecondsBetweenChecks { get; set; }

        public List<int> SecondsBetweenFailedChecks { get; set; }

        public Location Location { get; set; }

        public string NodeName { get; set; }
    }
}
