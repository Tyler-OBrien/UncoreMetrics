using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Ping_Collector_Probe.Models;
using Ping_Collector_Probe.Models.Resolvers;
using Ping_Collector_Probe.Services;
using Sentry;
using Serilog.Context;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData;

namespace Ping_Collector_Probe
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ProbeConfiguration _config;

        private readonly IScrapeJobStatusService _scrapeJobStatusService;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory,
            IOptions<ProbeConfiguration> probeConfiguration, IScrapeJobStatusService scrapeJobStatusService)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _config = probeConfiguration.Value;
            _scrapeJobStatusService = scrapeJobStatusService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Trying to register
                using (var scope = _scopeFactory.CreateScope())
                {
                    var pingCollectorApi = scope.ServiceProvider.GetRequiredService<IPingCollectorAPI>();
                    if (await pingCollectorApi.RegisterLocation(_config.Location) == false)
                    {
                        _logger.LogCritical("Error, cannot register location, aborting.");
                        throw new InvalidOperationException("Error, cannot register location, aborting.");
                    }
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    try
                    {
                        await RunActions();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occured in RunActions");
                    }

                    _logger.LogInformation("Finished Run...");
                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured in Worker ExecuteAsync");
                throw;
            }
        }

        private async Task RunActions()
        {
            _logger.LogInformation("----------------------");
            _logger.LogInformation("Starting Poll...");
            _logger.LogInformation("----------------------");
            var serversCount = await ProcessPings();
            _logger.LogInformation("----------------------");
            _logger.LogInformation("Poll Complete... Reached {serversCount} Servers.", serversCount);
            _logger.LogInformation("----------------------");
        }

        private async Task<int> ProcessPings()
        {
            using var scope = _scopeFactory.CreateScope();

            var pingApi = scope.ServiceProvider.GetRequiredService<IPingCollectorAPI>();

            var getServers = await pingApi.GetPingJobs(_config.Location.LocationID);
            if (getServers.Count < 1000)
            {
                _logger.LogInformation("Aborting run, less then 1000 servers to ping");
                return 0;
            }
            var getPings = await GetAllServerPings(getServers);

            getPings.ForEach(ping => ping.UpdateServerPing(_config.Location.LocationID, _config.SecondsBetweenChecks,
                _config.SecondsBetweenFailedChecks));
            // Submit Pings...
            await pingApi.SubmitPingJobs(new PingJobCompleteDTO() { CompletedPings = getPings });
            return getPings.Count;
        }


        private async Task<List<ServerPing>> GetAllServerPings(List<MiniServerDTO> servers)
        {
            string runType = "Ping";
            var stopwatch = Stopwatch.StartNew();
            using var cancellationTokenSource = new CancellationTokenSource();
            await _scrapeJobStatusService.StartRun(servers.Count, runType, cancellationTokenSource.Token);

            var maxConcurrency = _config.MaxConcurrency;

            _logger.LogInformation("Queueing Tasks");

            var newSolver = new DiscoverySolver(_logger, _config);

            using var queue = new AsyncResolveQueue<MiniServerDTO, ServerPing>(_logger,
                servers, maxConcurrency, newSolver,
                cancellationTokenSource.Token);

            var delayCount = 0;
            
            while (!queue.Done && delayCount <= _config.PollRunTimeout)
            {
                LogStatus(servers.Count, queue.Completed, queue.Failed, queue.Successful, maxConcurrency,
                    queue.Running);

                var scrapeJobUpdate = _scrapeJobStatusService.UpdateStatus(
                    (int)Math.Round(queue.Completed / (double)servers.Count * 100), queue.Completed, servers.Count,
                    runType, cancellationTokenSource.Token);
                await Task.WhenAll(Task.Delay(1000, cancellationTokenSource.Token), scrapeJobUpdate);
                delayCount++;
            }

            cancellationTokenSource.Cancel();
            if (delayCount >= _config.PollRunTimeout)
                _logger.LogWarning("[Warning] Operation timed out, reached {delayMax} Seconds, so we terminated. ",
                    _config.PollRunTimeout);
            var serverInfos = queue.Outgoing.ToList();

            stopwatch.Stop();
            _logger.LogInformation("Took {ElapsedMilliseconds}ms to get {ServersCount} Server infos from list",
                stopwatch.ElapsedMilliseconds, servers.Count);
            _logger.LogInformation("-----------------------------------------");
            _logger.LogInformation(
                "We were able to connect to {serverInfosCount} out of {serversCount} {successPercentage}%",
                serverInfos.Count, servers.Count, (int)Math.Round(serverInfos.Count / (double)servers.Count * 100));
            _logger.LogInformation(
                "Total Servers: {serverInfosCount}",
                serverInfos.Count);
            await _scrapeJobStatusService.EndRun(runType);

            return serverInfos;
        }

        private void LogStatus(int tasksCount, int totalCompleted, int failed,
            int successfullyCompleted, int concurrencyLimit, int totalQueued = 0)
        {
            _logger.LogInformation("Status Update: ");
            ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
            _logger.LogInformation(
                "Threads: {ThreadCount} Threads, Available Workers: {workerThreads}, Available Completion: {completionPortThreads}",
                ThreadPool.ThreadCount, workerThreads, completionPortThreads);
            if (tasksCount != 0)
            {
                _logger.LogInformation(
                    "Finished {totalCompleted}/{tasksCount} ({percentCompleted}%)", totalCompleted, tasksCount,
                    (int)Math.Round(totalCompleted / (double)tasksCount * 100));
                if (totalQueued != 0)
                    _logger.LogInformation(
                        "Queued: {totalQueued}/{concurrencyLimit} ({percentQueuedOfLimit}%)", totalQueued,
                        concurrencyLimit,
                        (int)Math.Round(totalQueued / (double)concurrencyLimit * 100));
            }

            if (failed != 0)
                _logger.LogInformation("Failed: {failed}, Successful {successfullyCompleted} ({percentSuccessful}%)",
                    failed, successfullyCompleted,
                    (int)Math.Round(successfullyCompleted / (double)totalCompleted * 100));
            else
                _logger.LogInformation("Failed: {failed}, Successful {successfullyCompleted} ({percentSuccessful}%)",
                    failed, successfullyCompleted,
                    100);
        }
    }
    public class DiscoverySolver : IGenericAsyncSolver<MiniServerDTO, ServerPing>
    {
        private readonly ILogger _logger;
        private readonly ProbeConfiguration _config;


        public DiscoverySolver(ILogger logger, ProbeConfiguration probeConfiguration)
        {
            _logger = logger;
            _config = probeConfiguration;
        }

        public async Task<(ServerPing? item, bool success)> Solve(MiniServerDTO poolItem)
        {
            var server = poolItem;
            try
            {
                try
                {
                    using (Ping ping = new Ping())
                    {
                        var pingResponse = await ping.SendPingAsync(poolItem.Address, 5000);
                        if (pingResponse.Status == IPStatus.Success)
                        {
                            return (new ServerPing()
                            {
                                LastCheck = DateTime.UtcNow, PingMs = pingResponse.RoundtripTime,
                                ServerId = poolItem.ServerId, LocationID = _config.Location.LocationID
                            }, true);
                        }
                    }
                }
                catch (PingException) { /* Yum */ }
#if DEBUG
                _logger.LogDebug("Failed to get {Address}", server.Address);
#endif
                var tryGetOldPing =
                    server.ServerPings.FirstOrDefault(ping => ping.LocationID == _config.Location.LocationID)?.PingMs ?? 0;
                return (new ServerPing()
                {
                    LastCheck = DateTime.UtcNow,
                    Failed = true,
                    ServerId = poolItem.ServerId,
                    LocationID = _config.Location.LocationID,
                    PingMs = tryGetOldPing,
                }, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                SentrySdk.CaptureException(ex);
#if DEBUG
                _logger.LogDebug("Failed to get {Address}", server.Address);
#endif
            }

            return (null, success: false);
        }
    }
}