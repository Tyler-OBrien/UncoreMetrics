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
using Shared_Collectors.Models.Games.Steam.SteamAPI;

namespace Shared_Collectors.Games.Steam.Generic
{
    public partial class GenericSteamStats : IGenericSteamStats
    {
        /// <summary>
        ///     Handles generic Steam Stats, grabbing active servers from the Steam Web API Server List, grabbing Server info /
        ///     rules / players, and submitting to postgres & Clickhouse.
        /// </summary>
        /// <param name="appID"></param>
        /// <returns>Returns a list of full server info to be actioned on with stats for that specific server type</returns>
        public async Task<List<DiscoveredServerInfo>> GenericServerDiscovery(long appID)
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
            await _genericServersContext.BulkSaveChangesAsync(bulkConfig, obj =>
            {
#if DEBUG
                Console.WriteLine($"BulkSaveChangesAsync Progress {obj}%...");
#endif
            });
        }



        private async Task<List<DiscoveredServerInfo>> GetAllServersDiscovery(List<SteamListServer> servers)
        {
            var serverInfos = new ConcurrentBag<DiscoveredServerInfo>();
            var stopwatch = Stopwatch.StartNew();




            // Might want to make this configurable eventually..
            var maxConcurrency = 2046;
            // Windows performs much worse with that many sockets..
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                maxConcurrency = 100;
            }


            var concurrencySemaphore = new SemaphoreSlim(maxConcurrency);





            var tasks = new ConcurrentBag<Task>();
            var successfullyCompleted = 0;
            var failed = 0;
            var totalCompleted = 0;

            await QueueTasks(tasks, servers, serverInfos, concurrencySemaphore, successfullyCompleted, failed,
                totalCompleted);

            
            var waitAll = Task.WhenAll(tasks.ToArray());
            while (await Task.WhenAny(waitAll, Task.Delay(1000)) != waitAll)
            {
                Console.Write("Status Update: ");
                ThreadPool.GetAvailableThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
                Console.WriteLine($"Threads: {ThreadPool.ThreadCount} Threads, maxWorker: {maxWorkerThreads}, maxCompletion: {maxCompletionPortThreads} ");
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

        public async Task QueueTasks(ConcurrentBag<Task> tasks, List<SteamListServer> servers, ConcurrentBag<DiscoveredServerInfo> serverInfos, SemaphoreSlim concurrencySemaphore, int successfullyCompleted, int failed, int totalCompleted)
        {
            Console.WriteLine("Queueing Tasks");

            foreach (var server in servers)
            {
                await concurrencySemaphore.WaitAsync();
                var newTask = Task.Run(async () =>
                {
                    try
                    {
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
                            serverInfos.Add(new DiscoveredServerInfo(Host, Port, server, info, players, rules, GetIPInformation));
                            Interlocked.Increment(ref successfullyCompleted);
                        }
                        else
                        {
#if DEBUG
                            Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.SteamID}");
#endif
                            Interlocked.Increment(ref failed);
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
                Console.WriteLine($"Finished queueing all {tasks.Count} tasks to get server info..");

            }
        }
    }
}
