using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncoreMetrics.Data.Configuration
{
    public class BaseConfiguration
    {
        public string PostgresConnectionString { get; set; }

        public string ClickhouseConnectionString { get; set; }

        public string SENTRY_DSN { get; set; }


    }
}
