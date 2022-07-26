using System.Text;
using Shared_Collectors.Games.Steam.Generic;
using Shared_Collectors.Helpers;
using Shared_Collectors.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Data.GameData.VRising;

namespace V_Rising_Collector;

public class Worker : BackgroundService
{
    private const ulong VRisingAppId = 1604030;

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
            var servers = await steamStats.GenericServerDiscovery<VRisingServer>(VRisingAppId);
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
            var servers = await steamStats.GenericServerPoll<VRisingServer>(VRisingAppId);
            servers.ForEach(ResolveCustomServerInfo);
            await steamStats.BulkInsertOrUpdate(servers.Select(server => server.CustomServerInfo).ToList());
            Console.WriteLine("----------------------");
            Console.WriteLine($"Poll Complete... Found {servers.Count} Servers.");
            Console.WriteLine("----------------------");
        }
    }

    // We might be able to implement this by just using attributes in the future
    private void ResolveCustomServerInfo(IGenericServerInfo<VRisingServer> server)
    {
        try
        {
            if (server.ServerRules != null)
            {

                if (server.ServerRules.TryGetBoolean("blood-bound-enabled", out var bloodBound))
                    server.CustomServerInfo.BloodBoundEquipment = bloodBound;
                if (server.ServerRules.TryGetEnum("castle-heart-damage-mode", out CastleHeartDamageMode castleHeartDamageMode))
                    server.CustomServerInfo.HeartDamage = castleHeartDamageMode;
                if (server.ServerRules.TryGetInt("days-runningv2", out var daysRunning))
                    server.CustomServerInfo.DaysRunning = daysRunning;
                if (server.ServerRules.TryGetRunningString("desc{0}", out var description))
                    server.CustomServerInfo.Description = description;

    
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected issue resolving custom server rules for VRising" + ex);
        }
    }
}