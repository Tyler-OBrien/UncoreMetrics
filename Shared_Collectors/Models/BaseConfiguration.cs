using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Collectors.Models
{
    public class BaseConfiguration
    {
        public string PostgresConnectionString { get; set; }

        public string ClickhouseConnectionString { get; set; }
    }
}
