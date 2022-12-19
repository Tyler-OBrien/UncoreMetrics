using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncoreMetrics.Data.Discord
{
    public class ServerUpdateNATs
    {
        public List<Guid> ServersUp { get; set; }

        public List<Guid> ServersDown { get; set; }
    }
}
