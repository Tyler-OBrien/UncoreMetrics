using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Collectors.Models.Tools.Maxmind
{
    public class IPInformation
    {
        public string? AutonomousSystemOrganization { get; set; }

        public long? AutonomousSystemNumber { get; set; }

        public string? LargestNetworkCIDR { get; set; }

        public string? Continent { get; set; }

        public string? Country { get; set; }

        public string? City { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string? TimeZone { get; set; }
    }
}
