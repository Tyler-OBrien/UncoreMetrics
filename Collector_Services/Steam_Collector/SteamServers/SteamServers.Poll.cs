using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Okolni.Source.Query.Source;
using Sentry;
using Serilog.Context;
using Steam_Collector.Helpers;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers.ServerQuery;
using UncoreMetrics.Data;

namespace Steam_Collector.SteamServers;

public class PollSolver : IGenericAsyncSolver<QueryPoolItem<Server>, PollServerInfo>
{
    private readonly ILogger _logger;

    public PollSolver( ILogger logger)
    {
        _logger = logger;
    }
    public async Task<(PollServerInfo? item, bool success)> Solve(QueryPoolItem<Server> item)
    {
        var server = item.Item;
        var pool = item.QueryConnectionPool;
        try
        {
            var endPoint = new IPEndPoint(server.Address, server.QueryPort);
            var info = await pool.GetServerInfoSafeAsync(endPoint);
            var rules = await pool.GetRulesSafeAsync(endPoint);
            var players = await pool.GetPlayersSafeAsync(endPoint);

            if (info != null && rules != null && players != null)
            {
                return (new PollServerInfo(server, info, players, rules), success: true);
            }
#if DEBUG
            _logger.LogDebug("Failed to get {Address} - {Name} - {LastCheck}", server.Address, server.Name, server.LastCheck);
#endif
            return (new PollServerInfo(server, info, players, rules), success: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            SentrySdk.CaptureException(ex);
#if DEBUG
            _logger.LogDebug("Failed to get {Address} - {Name} - {LastCheck}", server.Address, server.Name, server.LastCheck);
#endif
        }

        return (new PollServerInfo(server, null, null, null), success: false);
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
    public async Task<List<PollServerInfo>> GenericServerPoll(List<Server> servers)
    {

        var polledServers = await GetAllServersPoll(servers);



        return polledServers;
    }


    private async Task<List<PollServerInfo>> GetAllServersPoll(List<Server> servers)
    {
        const string runType = "Poll";
        using var context = LogContext.PushProperty("RunType", runType);
        var stopwatch = Stopwatch.StartNew();
        using var cancellationTokenSource = new CancellationTokenSource();
        await _scrapeJobStatusService.StartRun(servers.Count, runType, cancellationTokenSource.Token);

        // Might want to make this configurable eventually.. Right now Windows runs way worse then other platforms like Linux
        var maxConcurrency = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 512 : 1024;

        var newSolver = new PollSolver(_logger);
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
        using var queue = new AsyncResolveQueue<QueryPoolItem<Server>, PollServerInfo>(_logger,
            servers.Select(server => new QueryPoolItem<Server>(pool, server)), maxConcurrency, newSolver, cancellationTokenSource.Token);

        // Wait a max of 60 seconds...
        var delayCount = 0;
        while (!queue.Done && delayCount <= 90)
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
        if (delayCount >= 90)
            _logger.LogWarning("[Warning] Operation timed out, reached {delayMax} Seconds, so we terminated. ", delayCount);
        var serverInfos = queue.Outgoing;


        stopwatch.Stop();
        _logger.LogInformation("Took {ElapsedMilliseconds}ms to get {ServersCount} Server infos from list", stopwatch.ElapsedMilliseconds, servers.Count);
        _logger.LogInformation("-----------------------------------------");
        _logger.LogInformation(
            "We were able to connect to {serverInfosCount} out of {serversCount} {successPercentage}%", serverInfos.Count, servers.Count, (int)Math.Round(serverInfos.Count / (double)servers.Count * 100));
        _logger.LogInformation(
            "Total Players: {playersCount}, Total Servers: {serverInfosCount}", serverInfos.Sum(info => info.ServerInfo?.Players), serverInfos.Count);
        await _scrapeJobStatusService.EndRun(runType);
        return serverInfos.ToList();
    }
}