using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using EFCore.BulkExtensions;
using Shared_Collectors.Games.Steam.Generic.ServerQuery;
using Shared_Collectors.Games.Steam.Generic.WebAPI;
using Shared_Collectors.Models.Games.Steam.SteamAPI;
using Shared_Collectors.Tools.Maxmind;
using UncoreMetrics.Data;

namespace Shared_Collectors.Games.Steam.Generic;

public class GenericSteamStats : IGenericSteamStats
{
    private readonly GenericServersContext _genericServersContext;
    private readonly IGeoIPService _geoIpService;
    private readonly ISteamAPI _steamApi;

    public GenericSteamStats(ISteamAPI steamAPI, IGeoIPService geoIPService, GenericServersContext serversContext)
    {
        _steamApi = steamAPI;
        _geoIpService = geoIPService;
        _genericServersContext = serversContext;
    }

    /// <summary>
    ///     Handles generic Steam Stats, grabbing active servers from the Steam Web API Server List, grabbing Server info /
    ///     rules / players, and submitting to postgres & Clickhouse.
    /// </summary>
    /// <param name="appID"></param>
    /// <returns>Returns a list of full server info to be actioned on with stats for that specific server type</returns>
    public async Task<List<FullServerInfo>> HandleGeneric(string appID)
    {
        if (_steamApi == null) throw new NullReferenceException("Steam API cannot be null to use HandleGeneric");

        // We should move to something more like EF Core

        // Worth noting, this will only get us 20k Servers max. Querying the Master Server Query List directly leads to too many timeouts though, 20k is more then enough servers if we include only ones with players.
        var serverList =
            await _steamApi.GetServerList(SteamServerListQueryBuilder.New().AppID(appID).Dedicated().NotEmpty(),
                int.MaxValue);

        var servers = await GetAllServers(serverList);

        // Some submission step....
        await Submit(servers);

        return servers;
    }

    /// <summary>
    ///     Handles generic Steam Stats, grabbing Server info / rules / players, and submitting to Postgres & Clickhouse.
    /// </summary>
    /// <param name="servers"></param>
    /// <returns>Returns a list of full server info to be actioned on with stats for that specific server type</returns>
    public async Task<List<FullServerInfo>> HandleGeneric(List<SteamListServer> steamListServers)
    {
        // We should move to something more like EF Core


        var servers = await GetAllServers(steamListServers);

#if DEBUG
        Console.WriteLine($"Got {servers.Count}, now submitting....");
#endif
        // Some submission step....
        await Submit(servers);
#if DEBUG
        Console.WriteLine($"Submitted {servers.Count} to Database..");
#endif

        return servers;
    }

    private async Task Submit(List<FullServerInfo> fullServers)
    {
        var genericServers = fullServers.Select(fullserver => fullserver.ToGenericServer()).ToList();
        await _genericServersContext.BulkInsertOrUpdateAsync(genericServers);
        await _genericServersContext.BulkSaveChangesAsync();
    }


    private async Task<List<FullServerInfo>> GetAllServers(List<SteamListServer> servers)
    {
        var serverInfos = new ConcurrentBag<FullServerInfo>();
        var stopwatch = Stopwatch.StartNew();
        // Might want to make this configurable eventually..
        var maxConcurrency = 2046;
        var concurrencySemaphore = new SemaphoreSlim(maxConcurrency);

        var tasks = new ConcurrentBag<Task>();
        var successfullyCompleted = 0;
        var failed = 0;
        var totalCompleted = 0;
        Console.WriteLine("Queueing Tasks");
        foreach (var server in servers)
        {
            var newTask = Task.Run(async () =>
            {
                try
                {
                    await concurrencySemaphore.WaitAsync();
                    var (Host, Port) = SteamServerQuery.ParseIPAndPort(server.Address);
                    var HostStr = Host.ToString();
                    var infoTask = SteamServerQuery.GetServerInfo(HostStr, Port);
                    var rulesTask = SteamServerQuery.GetRules(HostStr, Port);
                    var playersTask = SteamServerQuery.GetPlayers(HostStr, Port);
                    var GetIPInformation = await _geoIpService.GetIpInformation(HostStr);
                    await Task.WhenAll(infoTask, rulesTask, playersTask);
                    var (info, rules, players) = (infoTask.Result, rulesTask.Result, playersTask.Result);
                    if (info != null && rules != null && players != null)
                    {
                        Interlocked.Increment(ref successfullyCompleted);
                    }
                    else
                    {
#if DEBUG
                        Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.SteamID}");
#endif
                        Interlocked.Increment(ref failed);
                    }

                    if (info != null)
                    {
                        serverInfos.Add(new FullServerInfo(Host, Port, server, info, players, rules, GetIPInformation));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error" + ex.ToString());
#if DEBUG
                    Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.SteamID}");
#endif
                    Interlocked.Increment(ref failed);
                }
                finally
                {
                    Interlocked.Increment(ref totalCompleted);

                    concurrencySemaphore.Release();
                }
                
            });

            tasks.Add(newTask);
        }

        Console.WriteLine($"Finished queueing all {tasks.Count} tasks to get server info..");


        var waitAll = Task.WhenAll(tasks.ToArray());
        while (await Task.WhenAny(waitAll, Task.Delay(1000)) != waitAll)
        {
            Console.Write("Status Update: ");
            if (tasks.Count != 0)
                Console.Write(
                    $"Finished {totalCompleted}/{tasks.Count} ({(int)Math.Round(totalCompleted / (double)tasks.Count * 100)}%)");

            Console.Write($" Failed: {failed}, Successful {successfullyCompleted}");
            if (failed != 0)
                Console.WriteLine($" ({(int)Math.Round(successfullyCompleted / (double)totalCompleted * 100)}%)");
            else
                Console.WriteLine(" (100%)");
        }

        concurrencySemaphore.Dispose();


        stopwatch.Stop();
        Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds}ms to get {servers.Count} server infos from list");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine(
            $"We were able to connect to {serverInfos.Count} out of {servers.Count} {(int)Math.Round(serverInfos.Count / (double)servers.Count * 100)}%");
        Console.WriteLine(
            $"Total Players: {serverInfos.Sum(info => info.serverInfo?.Players)}, Total Servers: {serverInfos.Count}");
        return serverInfos.ToList();
    }
}