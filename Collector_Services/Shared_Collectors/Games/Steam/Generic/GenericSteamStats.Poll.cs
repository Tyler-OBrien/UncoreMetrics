using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Okolni.Source.Query.Source;
using Shared_Collectors.Games.Steam.Generic.ServerQuery;
using Shared_Collectors.Helpers;
using Shared_Collectors.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Data;

namespace Shared_Collectors.Games.Steam.Generic;

public class PollSolver<T> : IGenericAsyncSolver<QueryPoolItem<GenericServer>, PollServerInfo<T>>
    where T : GenericServer, new()
{
    public async Task<PollServerInfo<T>?> Solve(QueryPoolItem<GenericServer> item)
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
                return new PollServerInfo<T>(server, info, players, rules);
            }
#if DEBUG
            Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.LastCheck}");
#endif
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected error" + ex);
#if DEBUG
            Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.LastCheck}");
#endif
            return null;
        }

        return null;
    }
}

public partial class GenericSteamStats : IGenericSteamStats
{
    /// <summary>
    ///     Handles generic Steam Stats, grabbing active servers from the Steam Web API Server List, grabbing Server info /
    ///     rules / players, and submitting to postgres & Clickhouse.
    /// </summary>
    /// <param name="appID"></param>
    /// <returns>Returns a list of full server info to be actioned on with stats for that specific server type</returns>
    public async Task<List<PollServerInfo<T>>> GenericServerPoll<T>(ulong appID) where T : GenericServer, new()
    {
        if (_steamApi == null) throw new NullReferenceException("Steam API cannot be null to use HandleGeneric");

        var servers = await _genericServersContext.Servers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == appID).ToListAsync();


        var polledServers = await GetAllServersPoll<T>(servers);


        foreach (var server in polledServers)
            server.CreateCustomServerInfo(_configuration.SecondsBetweenChecks,
                _configuration.SecondsBetweenFailedChecks, _configuration.DaysUntilServerMarkedDead);


        return polledServers;
    }


    private async Task<List<PollServerInfo<T>>> GetAllServersPoll<T>(List<GenericServer> servers)
        where T : GenericServer, new()
    {
        var stopwatch = Stopwatch.StartNew();
        // Might want to make this configurable eventually..
        var maxConcurrency = 512;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            // Linux doesn't seem to need one thread per connection..
            maxConcurrency = 1024;

        var newSolver = new PollSolver<T>();
        var pool = new QueryConnectionPool();
        pool.ReceiveTimeout = 750;
        pool.SendTimeout = 750;
        pool.Message += msg => { Console.WriteLine("Pool Message" + msg); };
        pool.Error += exception =>
        {
            Console.WriteLine("Exception from pool: " + exception);
            throw exception;
        };
        pool.Setup();
        var queue = new AsyncResolveQueue<QueryPoolItem<GenericServer>, PollServerInfo<T>>(
            servers.Select(server => new QueryPoolItem<GenericServer>(pool, server)), maxConcurrency, newSolver);

        // Wait a max of 60 seconds...
        var delayCount = 0;
        while (!queue.Done && delayCount <= 500)
        {
            LogStatus(pool, servers.Count, queue.Completed, queue.Failed, queue.Successful, maxConcurrency,
                queue.Running);
            await Task.Delay(1000);
            delayCount++;
        }

        if (delayCount >= 60)
            Console.WriteLine($"[Warning] Operation timed out, reached {delayCount} Seconds, so we terminated. ");
        queue.Dispose();
        var serverInfos = queue.Outgoing;
        pool.Dispose();


        stopwatch.Stop();
        Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds}ms to get {servers.Count} server infos from list");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine(
            $"We were able to connect to {serverInfos.Count} out of {servers.Count} {(int)Math.Round(serverInfos.Count / (double)servers.Count * 100)}%");
        Console.WriteLine(
            $"Total Players: {serverInfos.Sum(info => info.ServerInfo?.Players)}, Total Servers: {serverInfos.Count}");
        return serverInfos.ToList();
    }
}