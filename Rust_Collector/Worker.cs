using System.Text;
using Shared_Collectors.Games.Steam.Generic;
using Shared_Collectors.Helpers;
using Shared_Collectors.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Data.GameData.ARK;

namespace Rust_Collector;

public class Worker : BackgroundService
{
    private const ulong AppId = 346110;

    public const int SECONDS_BETWEEN_DISCOVERY = 600;
    private readonly ILogger<Worker> _logger;

    private readonly IServiceScopeFactory _scopeFactory;

    private DateTime _nextDiscoveryTime = DateTime.UnixEpoch;


    public Worker(ILogger<Worker> logger, IServiceScopeFactory steamStats)
    {
        _logger = logger;
        _scopeFactory = steamStats;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await RunActions();
            //await AltRun();
            Console.WriteLine("Finished Run...");
            await Task.Delay(30000, stoppingToken);
        }
    }


    private async Task RunActions()
    {
        using var scope = _scopeFactory.CreateScope();

        var steamStats = scope.ServiceProvider.GetService<IGenericSteamStats>();


        if (_nextDiscoveryTime < DateTime.UtcNow)
        {
            Console.WriteLine("----------------------");
            Console.WriteLine("Starting Discovery...");
            Console.WriteLine("----------------------");
            _nextDiscoveryTime = DateTime.UtcNow.AddSeconds(SECONDS_BETWEEN_DISCOVERY);
            var servers = await steamStats.GenericServerDiscovery<ArkServer>(AppId);
            servers.ForEach(ResolveCustomServerInfo);
            await steamStats.BulkInsertOrUpdate(servers.Select(server => server.CustomServerInfo).ToList());
            Console.WriteLine("----------------------");
            Console.WriteLine($"Discovery Complete... Found {servers.Count} Servers.");
            Console.WriteLine("----------------------");
        }
        else
        {
            Console.WriteLine("----------------------");
            Console.WriteLine("Starting Poll...");
            Console.WriteLine("----------------------");
            var servers = await steamStats.GenericServerPoll<ArkServer>(AppId);
            servers.ForEach(ResolveCustomServerInfo);
            await steamStats.BulkInsertOrUpdate(servers.Select(server => server.CustomServerInfo).ToList());
            Console.WriteLine("----------------------");
            Console.WriteLine($"Poll Complete... Found {servers.Count} Servers.");
            Console.WriteLine("----------------------");
        }
    }

    // We might be able to implement this by just using attributes in the future
    private void ResolveCustomServerInfo(IGenericServerInfo<ArkServer> server)
    {
        try
        {
            if (server.ServerRules != null)
            {

                if (server.ServerRules.TryGetBooleanExtended("ALLOWDOWNLOADCHARS_i", out var allowDownloadChars))
                    server.CustomServerInfo.DownloadCharacters = allowDownloadChars;


                if (server.ServerRules.TryGetBooleanExtended("ALLOWDOWNLOADITEMS_i", out var allowDownloadItems))
                    server.CustomServerInfo.DownloadItems = allowDownloadItems;


                if (server.ServerRules.TryGetBooleanExtended("OFFICIALSERVER_i", out var officialServer))
                    server.CustomServerInfo.OfficialServer = officialServer;

                if (server.ServerRules.TryGetBooleanExtended("SESSIONISPVE_i", out var PVEServer))
                    server.CustomServerInfo.PVE = PVEServer;


                if (server.ServerRules.TryGetBooleanExtended("SERVERUSESBATTLEYE_b", out var hasBattleye))
                    server.CustomServerInfo.Battleye = hasBattleye;


                if (server.ServerRules.TryGetBooleanExtended("ServerPassword_b", out var hasPassword))
                    server.CustomServerInfo.PasswordRequired = hasPassword;

                if (server.ServerRules.TryGetBooleanExtended("HASACTIVEMODS_i", out var hasMods))
                    server.CustomServerInfo.Modded = hasMods;


                if (server.ServerRules.TryGetInt("DayTime_s", out var daysRunning))
                    server.CustomServerInfo.DaysRunning = daysRunning;


                if (server.ServerRules.TryGetInt("SESSIONFLAGS", out var sessionFlags))
                    server.CustomServerInfo.SessionFlags = sessionFlags;


                if (server.ServerRules.TryGetString("ClusterId_s", out var clusterId))
                    server.CustomServerInfo.ClusterID = clusterId;



                if (server.ServerRules.TryGetString("CUSTOMSERVERNAME_s", out var customServerName))
                    server.CustomServerInfo.CustomServerName = customServerName;


                server.CustomServerInfo.Mods = server.ServerRules.TryGetRunningList("MOD{0}_s");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected issue resolving custom server rules for VRising" + ex);
        }
    }
}