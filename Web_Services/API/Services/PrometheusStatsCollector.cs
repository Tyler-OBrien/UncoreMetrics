using Microsoft.EntityFrameworkCore;
using Prometheus;
using UncoreMetrics.API.Controllers;
using UncoreMetrics.API.Models.Services;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData;

namespace UncoreMetrics.API.Services
{
    public class PrometheusStatsCollector : IPrometheusStatsCollector
    {
        private readonly IClickHouseService _clickHouseService;

        private readonly ServersContext _genericServersContext;
        private readonly ILogger _logger;


        private static readonly Gauge JobsRunning = Metrics
            .CreateGauge("uncore_jobs_running", "Number of scrape jobs running");


        private static readonly Gauge ServersInQueue = Metrics
            .CreateGauge("uncore_servers_overdue", "Servers overdue checks");

        private static readonly Gauge PingsInQueue = Metrics
            .CreateGauge("uncore_ping_overdue", "Pings Overdue");


        private static readonly Gauge ServersCheckedInTheLast5Minutes = Metrics
            .CreateGauge("uncore_servers_5m_checks", "Servers checked in the last 5 minutes Postgres");


        private static readonly Gauge OldestJobNotUpdated = Metrics
            .CreateGauge("uncore_oldest_job_time", "Job with oldest update time");

        private static readonly Dictionary<ulong, Gauge> AppIDGauges = new Dictionary<ulong, Gauge>();




        public PrometheusStatsCollector(ServersContext serversContext, IClickHouseService clickHouse,
            ILogger<PrometheusStatsCollector> logger)
        {
            _genericServersContext = serversContext;
            _clickHouseService = clickHouse;
            _logger = logger;
        }
        public async Task AddStats(CancellationToken token)
        {
            var getOldestJob = await _genericServersContext.ScrapeJobs
                .OrderByDescending(job => DateTime.UtcNow - job.LastUpdateUtc).FirstOrDefaultAsync(token);
            var getLastUpdateForOldestJob = (DateTime.UtcNow - (getOldestJob)?.LastUpdateUtc)?.TotalSeconds;
            OldestJobNotUpdated.Set(getLastUpdateForOldestJob ?? 0);
            JobsRunning.Set(await _genericServersContext.ScrapeJobs.CountAsync(job => job.Running, token));
            ServersInQueue.Set(await _genericServersContext.Servers.CountAsync(server => server.NextCheck < DateTime.UtcNow && server.ServerDead == false, token));
            PingsInQueue.Set(await _genericServersContext.PingData.CountAsync(server =>  server.NextCheck < DateTime.UtcNow, token));
            ServersCheckedInTheLast5Minutes.Set(await _genericServersContext.Servers.CountAsync(server => server.LastCheck > DateTime.UtcNow - TimeSpan.FromMinutes(5) && server.ServerDead == false, token));
            var serversGroupedByType = await _genericServersContext.Servers.Where(server => server.LastCheck > DateTime.UtcNow - TimeSpan.FromMinutes(5) && server.ServerDead == false).GroupBy(server => server.AppID).Select(group => new { name = group.Key, count = group.Count() }).ToListAsync(token);
            foreach (var serverGroup in serversGroupedByType)
            {
                Gauge appidGauge = null;
                var resolveId = StaticGameInfo.Games.FirstOrDefault(game => game.AppId == serverGroup.name);
                if (AppIDGauges.TryGetValue(serverGroup.name, out appidGauge) == false)
                {
                    appidGauge = Metrics
                        .CreateGauge($"uncore_{serverGroup.name}_5m_checks", $"{resolveId?.Name}/{serverGroup.name} servers updated in the last 5 minutes");
                    AppIDGauges.Add(serverGroup.name, appidGauge);
                }
                appidGauge.Set(serverGroup.count);
            }
        }
    }
}
