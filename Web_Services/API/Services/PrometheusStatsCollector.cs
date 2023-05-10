using Microsoft.EntityFrameworkCore;
using Prometheus;
using UncoreMetrics.API.Controllers;
using UncoreMetrics.API.Models.Services;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data;

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



        public PrometheusStatsCollector(ServersContext serversContext, IClickHouseService clickHouse,
            ILogger<PrometheusStatsCollector> logger)
        {
            _genericServersContext = serversContext;
            _clickHouseService = clickHouse;
            _logger = logger;
        }
        public async Task AddStats(CancellationToken token)
        {
            JobsRunning.Set(await _genericServersContext.ScrapeJobs.CountAsync(job => job.Running, token));
            ServersInQueue.Set(await _genericServersContext.Servers.CountAsync(server => server.NextCheck > DateTime.UtcNow && server.ServerDead == false, token));
            PingsInQueue.Set(await _genericServersContext.PingData.CountAsync(server => server.NextCheck > DateTime.UtcNow, token));
            ServersCheckedInTheLast5Minutes.Set(await _genericServersContext.Servers.CountAsync(server => server.LastCheck > DateTime.UtcNow - TimeSpan.FromMinutes(5) && server.ServerDead == false, token));
        }
    }
}
