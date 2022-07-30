using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data.ClickHouse.Data;
using UncoreMetrics.Data.ClickHouse.Models;
using UncoreMetrics.Data.Configuration;

namespace UncoreMetrics.Data.ClickHouse
{
    public class ClickHouseService : IClickHouseService
    {
        private readonly BaseConfiguration _baseConfiguration;
        
        private readonly ClickHouseServer _server;



        public ClickHouseService(IOptions<BaseConfiguration> baseConfigurationOptions)
        {
            _baseConfiguration = baseConfigurationOptions.Value;
            _server = new ClickHouseServer(_baseConfiguration.ClickhouseConnectionString);
        }


        public Task Insert(IEnumerable<ClickHouseGenericServer> servers) => _server.Insert(servers);

    }
}
