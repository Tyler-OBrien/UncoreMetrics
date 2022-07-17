using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Shared_Collectors.Games.Steam.Generic.ServerQuery;
using Shared_Collectors.Games.Steam.Generic.WebAPI;
using Shared_Collectors.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Data;

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
        public async Task<List<PollServerInfo>> GenericServerPoll(long appID)
        {
            if (_steamApi == null) throw new NullReferenceException("Steam API cannot be null to use HandleGeneric");

            var servers = await _genericServersContext.Servers.Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == appID).ToListAsync();

            
            var polledServers = await GetAllServersPoll(servers);

            // Some submission step....
            await Submit(polledServers);

            return polledServers;
        }


        private async Task Submit(List<PollServerInfo> fullServers)
        {
            var bulkConfig = new BulkConfig()
            {
                PropertiesToExclude = new List<string>() { "SearchVector" },
                PropertiesToExcludeOnUpdate = new List<string>() { "FoundAt", "ServerID" }
            };



            var genericServers = fullServers.Select(fullserver => fullserver.UpdateGenericServer(_configuration.SecondsBetweenChecks, _configuration.SecondsBetweenFailedChecks, _configuration.DaysUntilServerMarkedDead)).ToList();
            await _genericServersContext.BulkUpdateAsync(genericServers, bulkConfig, obj =>
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



        private async Task<List<PollServerInfo>> GetAllServersPoll(List<GenericServer> servers)
        {
            var serverInfos = new ConcurrentBag<PollServerInfo>();
            var stopwatch = Stopwatch.StartNew();
            // Might want to make this configurable eventually..
            var maxConcurrency = 2046;
            // Windows performs much worse with that many sockets..
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                maxConcurrency = 512;
            }


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
                        var Port = server.Port;
                        var HostStr = server.Address.ToString();
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
                            Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.LastCheck}");
#endif
                            Interlocked.Increment(ref failed);
                        }
                        serverInfos.Add(new PollServerInfo(server, info, players, rules));

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unexpected error" + ex.ToString());
#if DEBUG
                        Console.WriteLine($"Failed to get {server.Address} - {server.Name} - {server.LastCheck}");
#endif
                        Interlocked.Increment(ref failed);
                        serverInfos.Add(new PollServerInfo(server, null, null, null));
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
}
