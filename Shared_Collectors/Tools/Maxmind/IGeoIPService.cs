using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared_Collectors.Models.Tools.Maxmind;

namespace Shared_Collectors.Tools.Maxmind
{
    public interface IGeoIPService
    {
        public ValueTask<IPInformation> GetIpInformation(string address);
    }
}
