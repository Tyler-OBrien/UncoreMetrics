using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using UncoreMetrics.Steam_Collector.Helpers.ScrapeJobStatus;
using UncoreMetrics.Steam_Collector.Models;

namespace UncoreMetrics.IntegrationTests;

public class ScrapeJobStatusTests(PostgresFixture postgres)
{
    private static ScrapeJobStatusService CreateService(UncoreMetrics.Data.ServersContext context, string node)
    {
        var configuration = PostgresFixture.Configuration;
        configuration.NodeName = node;
        return new ScrapeJobStatusService(Options.Create<SteamCollectorConfiguration>(configuration), context,
            NullLogger<ScrapeJobStatusService>.Instance);
    }

    [Fact]
    public async Task RunUpdates_KeepOneRowWithLatestProgress()
    {
        postgres.SkipIfUnavailable();
        var ct = TestContext.Current.CancellationToken;
        const string node = "status-test";

        await using (var context = postgres.CreateContext())
        {
            var service = CreateService(context, node);
            await service.StartRun(1000, "Poll", ct);
            await service.UpdateStatus(10, 100, 1000, "Poll", ct);
            await service.UpdateStatus(55, 550, 1000, "Poll", ct);

            await using var verify = postgres.CreateContext();
            var running = await verify.ScrapeJobs.AsNoTracking().SingleAsync(job => job.Node == node, ct);
            Assert.True(running.Running);
            Assert.Equal(55, running.Progress);
            Assert.Equal(550, running.TotalDone);

            await service.EndRun("Poll", ct);
        }

        // A later run (new scope, new service) finds and reuses the same row
        await using (var context = postgres.CreateContext())
        {
            var service = CreateService(context, node);
            await service.StartRun(500, "Discovery", ct);
        }

        await using var final = postgres.CreateContext();
        var job = await final.ScrapeJobs.AsNoTracking().SingleAsync(job => job.Node == node, ct);
        Assert.Equal("Discovery", job.RunType);
        Assert.Equal(500, job.TotalCount);
        Assert.True(job.Running);
    }
}
