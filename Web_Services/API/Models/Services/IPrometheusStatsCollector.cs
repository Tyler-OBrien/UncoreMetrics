namespace UncoreMetrics.API.Models.Services
{
    public interface IPrometheusStatsCollector
    {

        public Task AddStats(CancellationToken token);
    }
}
