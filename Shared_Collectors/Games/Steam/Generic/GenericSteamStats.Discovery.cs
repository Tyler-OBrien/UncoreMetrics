using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Shared_Collectors.Games.Steam.Generic.ServerQuery;
using Shared_Collectors.Games.Steam.Generic.WebAPI;
using Shared_Collectors.Helpers;
using Shared_Collectors.Models.Games.Steam.SteamAPI;
using Shared_Collectors.Tools.Maxmind;

namespace Shared_Collectors.Games.Steam.Generic
{
    public class DiscoverySolver : IGenericAsyncSolver<SteamListServer, DiscoveredServerInfo>
    {
        private readonly IGeoIPService _geoIpService;

        public DiscoverySolver(IGeoIPService geoIpService)
        {
            _geoIpService = geoIpService;
        }

        public async Task<DiscoveredServerInfo?> Solve(SteamListServer server)
        {
            try
            {
                var (Host, Port) = SteamServerQuery.ParseIPAndPort(server.Address);
                var HostStr = Host.ToString();
                var info = await SteamServerQuery.GetServerInfoAsync(HostStr, Port);
                var rules  = await SteamServerQuery.GetRulesAsync(HostStr, Port);
                var players = await SteamServerQuery.GetPlayersAsync(HostStr, Port);
                var geoIpInformation = await _geoIpService.GetIpInformation(HostStr);
                if (info != null && rules != null && players != null)
                {
                    return new DiscoveredServerInfo(Host, Port, server, info, players, rules, geoIpInformation);
                }
                else
                {
#if DEBUG
                    Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.SteamID}");
#endif
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error" + ex.ToString());
#if DEBUG
                Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.SteamID}");
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
        public async Task<List<DiscoveredServerInfo>> GenericServerDiscovery(ulong appID)
        {
            if (_steamApi == null) throw new NullReferenceException("Steam API cannot be null to use HandleGeneric");

            // We should move to something more like EF Core

            // Worth noting, this will only get us 20k Servers max. Querying the Master Server Query List directly leads to too many timeouts though, 20k is more then enough servers if we include only ones with players.
            var serverList =
                await _steamApi.GetServerList(SteamServerListQueryBuilder.New().AppID(appID.ToString()).Dedicated().NotEmpty(),
                    int.MaxValue);

            var servers = await GetAllServersDiscovery(serverList);

            // Some submission step....
            await Submit(servers);

            return servers;
        }


        private async Task Submit(List<DiscoveredServerInfo> fullServers)
        {
            var bulkConfig = new BulkConfig()
            {
                PropertiesToExclude = new List<string>() { "SearchVector" },
                PropertiesToExcludeOnUpdate = new List<string>() { "FoundAt", "ServerID", "SearchVector" }
            };



            var genericServers = fullServers.Select(fullserver => fullserver.ToGenericServer(_configuration.SecondsBetweenChecks)).ToList();
            await _genericServersContext.BulkInsertOrUpdateAsync(genericServers, bulkConfig, obj =>
            {
#if DEBUG
                Console.WriteLine($"BulkInsertOrUpdateAsync Progress {obj}%...");
#endif
            }, null, CancellationToken.None);
        }



        private async Task<List<DiscoveredServerInfo>> GetAllServersDiscovery(List<SteamListServer> servers)
        {
            var stopwatch = Stopwatch.StartNew();




            // Might want to make this configurable eventually..
            var maxConcurrency = 100;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux doesn't seem to need one thread per connection..
                maxConcurrency = 1024;
            }




            Console.WriteLine("Queueing Tasks");

            var newSolver = new DiscoverySolver(_geoIpService);

            var queue = new AsyncResolveQueue<SteamListServer, DiscoveredServerInfo>(servers, maxConcurrency, newSolver);

            while (!queue.Done)
            {
                LogStatus(servers.Count, queue.Completed, queue.Failed, queue.Successful, maxConcurrency,
                    queue.Running);
                await Task.Delay(1000);
            }
            queue.Dispose();
            var serverInfos = queue.Outgoing.ToList();


            stopwatch.Stop();
            Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds}ms to get {servers.Count} server infos from list");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine(
                $"We were able to connect to {serverInfos.Count} out of {servers.Count} {(int)Math.Round(serverInfos.Count / (double)servers.Count * 100)}%");
            Console.WriteLine(
                $"Total Players: {serverInfos.Sum(info => info.serverInfo?.Players)}, Total Servers: {serverInfos.Count}");
            return serverInfos;
        }

        private void LogStatus(int tasksCount, int totalCompleted, int failed, int successfullyCompleted, int concurrencyLimit, int totalQueued = 0)
        {
            Console.Write("Status Update: ");
            ThreadPool.GetAvailableThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
            Console.WriteLine($"Threads: {ThreadPool.ThreadCount} Threads, maxWorker: {maxWorkerThreads}, maxCompletion: {maxCompletionPortThreads} ");
            if (tasksCount != 0)
            {
                Console.Write(
                    $"Finished {totalCompleted}/{tasksCount} ({(int)Math.Round(totalCompleted / (double)tasksCount * 100)}%)");
                if (totalQueued != 0)
                {
                    Console.Write($" Queued: {totalQueued}/{concurrencyLimit} ({(int)Math.Round((totalQueued) / (double)(concurrencyLimit) * 100)}%) ");
                }

            }

            Console.Write($" Failed: {failed}, Successful {successfullyCompleted}");
            if (failed != 0)
                Console.WriteLine($" ({(int)Math.Round(successfullyCompleted / (double)totalCompleted * 100)}%)");
            else
                Console.WriteLine(" (100%)");
        }

    }
}
