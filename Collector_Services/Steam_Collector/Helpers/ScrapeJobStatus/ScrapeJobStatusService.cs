using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Options;
using Sentry;
using Steam_Collector.Models;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;

namespace Steam_Collector.Helpers.ScrapeJobStatus
{

    public class ScrapeJobStatusService : IScrapeJobStatusService
    {
        private readonly SteamCollectorConfiguration _configuration;
        private readonly ServersContext _genericServersContext;
        private readonly ILogger _logger;

        private static int _runid;
        private static Guid _runGuid;
        private static DateTime _startedAt;



        public ScrapeJobStatusService(
            IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext, ILogger<ScrapeJobStatusService> logger)
        {
            _genericServersContext = serversContext;
            _configuration = baseConfiguration.Value;
            _logger = logger;
        }

        public async Task StartRun(int totalCount, string runType, CancellationToken token = default)
        {
            _runGuid = Guid.NewGuid();
            _startedAt = DateTime.UtcNow;
            var newScrapeJob = new ScrapeJob(_configuration.GameType, runType, _configuration.NodeName, _runid, 0,
                0, totalCount, _runGuid, true, _startedAt, DateTime.UtcNow);
            await UpdateStatus(newScrapeJob, token);
        }

        public async Task  EndRun(string runType, CancellationToken token = default)
        {
            var newScrapeJob = new ScrapeJob(_configuration.GameType, runType, _configuration.NodeName, _runid, 0,
                0, 0, _runGuid, false, _startedAt, DateTime.UtcNow);
            _runid++;
            await UpdateStatus(newScrapeJob, token);
        }

        public async Task UpdateStatus(int progress, int totalDone, int totalCount, string runType, CancellationToken token = default)
        {
            var newScrapeJob = new ScrapeJob(_configuration.GameType, runType, _configuration.NodeName, _runid, progress,
                totalDone, totalCount, _runGuid, true, _startedAt, DateTime.UtcNow);
            await UpdateStatus(newScrapeJob, token);
        }

        private async Task UpdateStatus(ScrapeJob job, CancellationToken token)
        {
            var bulkConfig = new BulkConfig
            {
            };
      
                await _genericServersContext.BulkInsertOrUpdateAsync(new ScrapeJob[] { job }, bulkConfig,
                    cancellationToken: token);
        }

    }
}
