using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared_Collectors.Games.Steam.Generic.ServerQuery;
using Shared_Collectors.Models.Games.Steam.SteamAPI;

namespace Shared_Collectors.Games.Steam.Generic
{
    public class GenericSteamStats
    {
        private readonly SteamAPI _steamApi;
        public GenericSteamStats(SteamAPI steamAPI)
        {
            _steamApi = steamAPI;
        }

        /// <summary>
        /// Handles generic Steam Stats, grabbing active servers from the Steam Web API Server List, grabbing Server info / rules / players, and submitting to postgres & Clickhouse.
        /// </summary>
        /// <param name="appID"></param>
        /// <returns>Returns a list of full server info to be actioned on with stats for that specific server type</returns>
        public async Task<List<FullServerInfo>> HandleGeneric(string appID)
        {


            // We should move to something more like EF Core

            // Worth noting, this will only get us 20k Servers max. Querying the Master Server Query List directly leads to too many timeouts though, 20k is more then enough servers if we include only ones with players.
            var serverList = await _steamApi.GetServerListAppID(appID, int.MaxValue);

            var servers = await GetAllServers(serverList);

            // Some submission step....

            return servers;
        }

        /// <summary>
        /// Handles generic Steam Stats, grabbing Server info / rules / players, and submitting to Postgres & Clickhouse.
        /// </summary>
        /// <param name="servers"></param>
        /// <returns>Returns a list of full server info to be actioned on with stats for that specific server type</returns>
        public async Task<List<FullServerInfo>> HandleGeneric(List<SteamListServer> steamListServers)
        {


            // We should move to something more like EF Core

  

            var servers = await GetAllServers(steamListServers);

            // Some submission step....

            return servers;
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

                        var infoTask = SteamServerQuery.GetServerInfo(server.Address);
                        var rulesTask = SteamServerQuery.GetRules(server.Address);
                        var playersTask = SteamServerQuery.GetPlayers(server.Address);
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
                        serverInfos.Add(new FullServerInfo(server, info, players, rules));

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
                    Console.Write($"Finished {totalCompleted}/{tasks.Count} ({(int)Math.Round(((double)totalCompleted / (double)tasks.Count) * 100)}%)");

                Console.Write($" Failed: {failed}, Successful {successfullyCompleted}");
                if (failed != 0)
                    Console.WriteLine($" ({(int)Math.Round(((double)successfullyCompleted / (double)totalCompleted) * 100)}%)");
                else
                    Console.WriteLine(" (100%)");
            }

            concurrencySemaphore.Dispose();


            stopwatch.Stop();
            Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds}ms to get {servers.Count} server infos from list");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine(
                $"We were able to connect to {serverInfos.Count} out of {servers.Count} {(int)Math.Round(((double)serverInfos.Count / (double)servers.Count) * 100)}%");
            Console.WriteLine($"Total Players: {serverInfos.Sum(info => info.serverInfo?.Players)}, Total Servers: {serverInfos.Count}");
            return serverInfos.ToList();
        }
    }
}
