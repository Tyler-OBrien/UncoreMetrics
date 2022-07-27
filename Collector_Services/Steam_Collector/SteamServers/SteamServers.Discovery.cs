using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Okolni.Source.Query.Source;
using Steam_Collector.Helpers;
using Steam_Collector.Helpers.IPAddressExtensions;
using Steam_Collector.Helpers.Maxmind;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers.ServerQuery;
using Steam_Collector.SteamServers.WebAPI;
using UncoreMetrics.Data;

namespace Steam_Collector.SteamServers;

public class DiscoverySolver : IGenericAsyncSolver<QueryPoolItem<SteamListServer>, DiscoveredServerInfo>
{
    private readonly IGeoIPService _geoIpService;

    public DiscoverySolver(IGeoIPService geoIpService)
    {
        _geoIpService = geoIpService;
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
            {
                return (new DiscoveredServerInfo(Host, Port, server, info, players, rules, geoIpInformation), success: true);
            }
#if DEBUG
            Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.SteamID}");
#endif
            return (null, success: false);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected error" + ex);
#if DEBUG
            Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.SteamID}");
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
    public async Task<List<DiscoveredServerInfo>> GenericServerDiscovery(ulong appID)
    {
        if (_steamApi == null) throw new NullReferenceException("Steam API cannot be null to use HandleGeneric");

        // We should move to something more like EF Core

        // Worth noting, this will only get us 20k Servers max. Querying the Master Server Query List directly leads to too many timeouts though, 20k is more then enough servers if we include only ones with players.
        var serverList =
            await _steamApi.GetServerList(
                SteamServerListQueryBuilder.New().AppID(appID.ToString()).Dedicated().NotEmpty(),
                int.MaxValue);
        var serverListCount = serverList.Count;
        foreach (var server in serverList.ToList())
        {
            var (Host, Port) = SteamServerQuery.ParseIPAndPort(server.Address);
            if (Host.IsPrivate())
            {
                serverList.Remove(server);
            }
        }
#if DEBUG
        Console.WriteLine($"Removed {serverListCount - serverList.Count} Servers containing Private IP Addresses (Valve Servers use Relays and don't expose, among others) ");
#endif

        var servers = await GetAllServersDiscovery(serverList);



        return servers;
    }


    private async Task<List<DiscoveredServerInfo>> GetAllServersDiscovery(List<SteamListServer> servers)
    {
        var stopwatch = Stopwatch.StartNew();


        // Might want to make this configurable eventually..
        var maxConcurrency = 512;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            // Linux doesn't seem to need one thread per connection..
            maxConcurrency = 1024;


        Console.WriteLine("Queueing Tasks");

        var newSolver = new DiscoverySolver(_geoIpService);
        var pool = new QueryConnectionPool();
        pool.ReceiveTimeout = 750;
        pool.SendTimeout = 750;
        pool.Message += msg => { Console.WriteLine("Pool Message: " + msg); };
        pool.Error += exception =>
        {
            Console.WriteLine("Exception from pool: " + exception);
            throw exception;
        };
        pool.Setup();

        var queue = new AsyncResolveQueue<QueryPoolItem<SteamListServer>, DiscoveredServerInfo>(
            servers.Select(server => new QueryPoolItem<SteamListServer>(pool, server)), maxConcurrency, newSolver);

        // Wait a max of 60 seconds...
        var delayCount = 0;
        while (!queue.Done && delayCount <= 60)
        {
            LogStatus(pool, servers.Count, queue.Completed, queue.Failed, queue.Successful, maxConcurrency,
                queue.Running);
            await Task.Delay(1000);
            delayCount++;
        }

        if (delayCount >= 60)
            Console.WriteLine($"[Warning] Operation timed out, reached {delayCount} Seconds, so we terminated. ");
        queue.Dispose();
        var serverInfos = queue.Outgoing.ToList();
        pool.Dispose();

        stopwatch.Stop();
        Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds}ms to get {servers.Count} Server infos from list");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine(
            $"We were able to connect to {serverInfos.Count} out of {servers.Count} {(int)Math.Round(serverInfos.Count / (double)servers.Count * 100)}%");
        Console.WriteLine(
            $"Total Players: {serverInfos.Sum(info => info.ServerInfo?.Players)}, Total Servers: {serverInfos.Count}");
        return serverInfos;
    }

    private void LogStatus(QueryConnectionPool pool, int tasksCount, int totalCompleted, int failed,
        int successfullyCompleted, int concurrencyLimit, int totalQueued = 0)
    {
        Console.Write("Status Update: ");
        ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
        Console.WriteLine(
            $"Threads: {ThreadPool.ThreadCount} Threads, Available Workers: {workerThreads}, Available Completion: {completionPortThreads}");
        Console.WriteLine(
            $"Connection Pool Running Queries: {pool.Running}, Pool Waiting Queries: {pool.WaitingForResponse}");
        if (tasksCount != 0)
        {
            Console.Write(
                $"Finished {totalCompleted}/{tasksCount} ({(int)Math.Round(totalCompleted / (double)tasksCount * 100)}%)");
            if (totalQueued != 0)
                Console.Write(
                    $" Queued: {totalQueued}/{concurrencyLimit} ({(int)Math.Round(totalQueued / (double)concurrencyLimit * 100)}%) ");
        }

        Console.Write($" Failed: {failed}, Successful {successfullyCompleted}");
        if (failed != 0)
            Console.WriteLine($" ({(int)Math.Round(successfullyCompleted / (double)totalCompleted * 100)}%)");
        else
            Console.WriteLine(" (100%)");
    }
}