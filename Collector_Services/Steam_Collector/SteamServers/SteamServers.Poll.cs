using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Okolni.Source.Query.Source;
using Steam_Collector.Helpers;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers.ServerQuery;
using UncoreMetrics.Data;

namespace Steam_Collector.SteamServers;

public class PollSolver : IGenericAsyncSolver<QueryPoolItem<Server>, PollServerInfo>
{
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
            Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.LastCheck}");
#endif
            return (new PollServerInfo(server, info, players, rules), success: false);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected error" + ex);
#if DEBUG
            Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.LastCheck}");
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
        var stopwatch = Stopwatch.StartNew();
        using var cancellationTokenSource = new CancellationTokenSource();

        // Might want to make this configurable eventually.. Right now Windows runs way worse then other platforms like Linux
        var maxConcurrency = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 512 : 1024;

        var newSolver = new PollSolver();
        using var pool = new QueryConnectionPool();
        pool.ReceiveTimeout = 750;
        pool.SendTimeout = 750;
        pool.Message += msg => { Console.WriteLine("Pool Message: " + msg); };
        pool.Error += exception =>
        {
            Console.WriteLine("Exception from pool: " + exception);
            throw exception;
        };
        pool.Setup();
        using var queue = new AsyncResolveQueue<QueryPoolItem<Server>, PollServerInfo>(
            servers.Select(server => new QueryPoolItem<Server>(pool, server)), maxConcurrency, newSolver, cancellationTokenSource.Token);

        // Wait a max of 60 seconds...
        var delayCount = 0;
        while (!queue.Done && delayCount <= 90)
        {
            LogStatus(pool, servers.Count, queue.Completed, queue.Failed, queue.Successful, maxConcurrency,
                queue.Running);
            await Task.Delay(1000);
            delayCount++;
        }
        cancellationTokenSource.Cancel();
        if (delayCount >= 90)
            Console.WriteLine($"[Warning] Operation timed out, reached {delayCount} Seconds, so we terminated. ");
        var serverInfos = queue.Outgoing;


        stopwatch.Stop();
        Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds}ms to get {servers.Count} Server infos from list");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine(
            $"We were able to connect to {serverInfos.Count} out of {servers.Count} {(int)Math.Round(serverInfos.Count / (double)servers.Count * 100)}%");
        Console.WriteLine(
            $"Total Players: {serverInfos.Sum(info => info.ServerInfo?.Players)}, Total Servers: {serverInfos.Count}");
        return serverInfos.ToList();
    }
}