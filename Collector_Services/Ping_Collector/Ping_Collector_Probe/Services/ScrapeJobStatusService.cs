using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ping_Collector_Probe.Models;
using UncoreMetrics.Data;

namespace Ping_Collector_Probe.Services
{
    public class ScrapeJobStatusService : IScrapeJobStatusService
    {
        private static int _runid;
        private static Guid _runGuid;
        private static DateTime _startedAt;
        private readonly ProbeConfiguration _configuration;
        private readonly IPingCollectorAPI _pingCollectorApi;
        private readonly ILogger _logger;

        private const string GAMETYPE = "Ping";


        public ScrapeJobStatusService(
            IOptions<ProbeConfiguration> baseConfiguration, IPingCollectorAPI pingCollectorApi,
            ILogger<ScrapeJobStatusService> logger)
        {
            _pingCollectorApi = pingCollectorApi;
            _configuration = baseConfiguration.Value;
            _logger = logger;
        }

        public async Task StartRun(int totalCount, string runType, CancellationToken token = default)
        {
            _runGuid = Guid.NewGuid();
            _startedAt = DateTime.UtcNow;
            var newScrapeJob = new ScrapeJob(GAMETYPE, runType, _configuration.NodeName, _runid, 0,
                0, totalCount, _runGuid, true, _startedAt, DateTime.UtcNow);
            await UpdateStatus(newScrapeJob, token);
        }

        public async Task EndRun(string runType, CancellationToken token = default)
        {
            var newScrapeJob = new ScrapeJob(GAMETYPE, runType, _configuration.NodeName, _runid, 0,
                0, 0, _runGuid, false, _startedAt, DateTime.UtcNow);
            _runid++;
            await UpdateStatus(newScrapeJob, token);
        }

        public async Task UpdateStatus(int progress, int totalDone, int totalCount, string runType,
            CancellationToken token = default)
        {
            var newScrapeJob = new ScrapeJob(GAMETYPE, runType, _configuration.NodeName, _runid, progress,
                totalDone, totalCount, _runGuid, true, _startedAt, DateTime.UtcNow);
            await UpdateStatus(newScrapeJob, token);
        }

        private async Task UpdateStatus(ScrapeJob job, CancellationToken token)
        {
            await _pingCollectorApi.SubmitScrapeJobUpdate(job);
        }
    }
}
