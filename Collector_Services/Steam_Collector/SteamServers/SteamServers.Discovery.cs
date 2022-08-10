using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Okolni.Source.Query.Source;
using Sentry;
using Serilog.Context;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Helpers.IPAddressExtensions;
using UncoreMetrics.Steam_Collector.Helpers.Maxmind;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers.ServerQuery;
using UncoreMetrics.Steam_Collector.SteamServers.WebAPI;

namespace UncoreMetrics.Steam_Collector.SteamServers;

public class DiscoverySolver : IGenericAsyncSolver<QueryPoolItem<SteamListServer>, DiscoveredServerInfo>
{
    private readonly IGeoIPService _geoIpService;
    private readonly ILogger _logger;

    public DiscoverySolver(IGeoIPService geoIpService, ILogger logger)
    {
        _geoIpService = geoIpService;
        _logger = logger;
    }

    public async Task<(DiscoveredServerInfo? item, bool success)> Solve(QueryPoolItem<SteamListServer> poolItem)
    {
        var server = poolItem.Item;
        var pool = poolItem.QueryConnectionPool;
        try
        {
            var (Host, Port) = SteamServerQuery.ParseIPAndPort(server.Address);
            var endPoint = new IPEndPoint(Host, Port);
            var info = await pool.GetServerInfoSafeAsync(endPoint);
            var rules = await pool.GetRulesSafeAsync(endPoint);
            var players = await pool.GetPlayersSafeAsync(endPoint);
            var geoIpInformation = await _geoIpService.GetIpInformation(Host.ToString());
            if (info != null && rules != null && players != null)
                return (new DiscoveredServerInfo(Host, Port, server, info, players, rules, geoIpInformation),
                    success: true);
#if DEBUG
            _logger.LogDebug("Failed to get {Address} - {Name} - {SteamID}", server.Address, server.Name,
                server.SteamID);
#endif
            return (null, success: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            SentrySdk.CaptureException(ex);
#if DEBUG
            _logger.LogDebug("Failed to get {Address} - {Name} - {SteamID}", server.Address, server.Name,
                server.SteamID);
#endif
        }

        return (null, success: false);
    }
}

public partial class SteamServers : ISteamServers
{
    /// <summary>
    ///     Handles generic Steam Stats, grabbing active servers from the Steam Web API Server List, grabbing Server info /
    ///     rules / players, and submitting to postgres & Clickhouse.
    /// </summary>
    /// <param name="appID"></param>
    /// <returns>Returns a list of full Server info to be actioned on with stats for that specific Server type</returns>
    public async Task<List<DiscoveredServerInfo>> GenericServerDiscovery(
        SteamServerListQueryBuilder queryListQueryBuilder)
    {
        if (_steamApi == null) throw new NullReferenceException("Steam API cannot be null to use HandleGeneric");


        // Worth noting, this will only get us 20k Servers max. Querying the Master Server Query List directly leads to too many timeouts though, 20k is more then enough servers if we include only ones with players.
        var serverList =
            await _steamApi.GetServerList(
                queryListQueryBuilder,
                int.MaxValue);

        var serverListCount = serverList.Count;
        foreach (var server in serverList.ToList())
        {
            var (Host, Port) = SteamServerQuery.ParseIPAndPort(server.Address);
            if (Host.IsPrivate()) serverList.Remove(server);
        }

        _logger.LogDebug(
            "Removed {removedPrivateIPServers} Servers containing Private IP Addresses (Valve Servers use Relays and don't expose, among others)",
            serverListCount - serverList.Count);

        var servers = await GetAllServersDiscovery(serverList);


        return servers;
    }


    private async Task<List<DiscoveredServerInfo>> GetAllServersDiscovery(List<SteamListServer> servers)
    {
        const string runType = "Discovery";
        using var context = LogContext.PushProperty("RunType", runType);
        var stopwatch = Stopwatch.StartNew();
        using var cancellationTokenSource = new CancellationTokenSource();
        await _scrapeJobStatusService.StartRun(servers.Count, runType, cancellationTokenSource.Token);

        // Might want to make this configurable eventually.. Right now Windows runs way worse then other platforms like Linux
        var maxConcurrency = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 512 : 1024;


        _logger.LogInformation("Queueing Tasks");

        var newSolver = new DiscoverySolver(_geoIpService, _logger);
        using var pool = new QueryConnectionPool();
        pool.ReceiveTimeout = 750;
        pool.SendTimeout = 750;
        pool.Message += msg => { _logger.LogInformation("Pool Message: {msg}", msg); };
        pool.Error += exception =>
        {
            _logger.LogError(exception, "Exception from pool");
            throw exception;
        };
        pool.Setup();

        using var queue = new AsyncResolveQueue<QueryPoolItem<SteamListServer>, DiscoveredServerInfo>(_logger,
            servers.Select(server => new QueryPoolItem<SteamListServer>(pool, server)), maxConcurrency, newSolver,
            cancellationTokenSource.Token);

        // Wait a max of 60 seconds...
        var delayCount = 0;
        while (!queue.Done && delayCount <= 60)
        {
            LogStatus(pool, servers.Count, queue.Completed, queue.Failed, queue.Successful, maxConcurrency,
                queue.Running);

            var scrapeJobUpdate = _scrapeJobStatusService.UpdateStatus(
                (int)Math.Round(queue.Completed / (double)servers.Count * 100), queue.Completed, servers.Count,
                runType, cancellationTokenSource.Token);
            await Task.WhenAll(Task.Delay(1000, cancellationTokenSource.Token), scrapeJobUpdate);
            delayCount++;
        }

        cancellationTokenSource.Cancel();
        if (delayCount >= 60)
            _logger.LogWarning("[Warning] Operation timed out, reached {delayMax} Seconds, so we terminated. ",
                delayCount);
        var serverInfos = queue.Outgoing.ToList();

        stopwatch.Stop();
        _logger.LogInformation("Took {ElapsedMilliseconds}ms to get {ServersCount} Server infos from list",
            stopwatch.ElapsedMilliseconds, servers.Count);
        _logger.LogInformation("-----------------------------------------");
        _logger.LogInformation(
            "We were able to connect to {serverInfosCount} out of {serversCount} {successPercentage}%",
            serverInfos.Count, servers.Count, (int)Math.Round(serverInfos.Count / (double)servers.Count * 100));
        _logger.LogInformation(
            "Total Players: {playersCount}, Total Servers: {serverInfosCount}",
            serverInfos.Sum(info => info.ServerInfo?.Players), serverInfos.Count);
        await _scrapeJobStatusService.EndRun(runType);

        return serverInfos;
    }

    private void LogStatus(QueryConnectionPool pool, int tasksCount, int totalCompleted, int failed,
        int successfullyCompleted, int concurrencyLimit, int totalQueued = 0)
    {
        _logger.LogInformation("Status Update: ");
        ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
        _logger.LogInformation(
            "Threads: {ThreadCount} Threads, Available Workers: {workerThreads}, Available Completion: {completionPortThreads}",
            ThreadPool.ThreadCount, workerThreads, completionPortThreads);
        _logger.LogInformation(
            "Connection Pool Running Queries: {poolRunning}, Pool Waiting Queries: {poolWaitingForResponse}",
            pool.Running, pool.WaitingForResponse);
        if (tasksCount != 0)
        {
            _logger.LogInformation(
                "Finished {totalCompleted}/{tasksCount} ({percentCompleted}%)", totalCompleted, tasksCount,
                (int)Math.Round(totalCompleted / (double)tasksCount * 100));
            if (totalQueued != 0)
                _logger.LogInformation(
                    "Queued: {totalQueued}/{concurrencyLimit} ({percentQueuedOfLimit}%)", totalQueued, concurrencyLimit,
                    (int)Math.Round(totalQueued / (double)concurrencyLimit * 100));
        }

        if (failed != 0)
            _logger.LogInformation("Failed: {failed}, Successful {successfullyCompleted} ({percentSuccessful}%)",
                failed, successfullyCompleted, (int)Math.Round(successfullyCompleted / (double)totalCompleted * 100));
        else
            _logger.LogInformation("Failed: {failed}, Successful {successfullyCompleted} ({percentSuccessful}%)",
                failed, successfullyCompleted, 100);
    }
}