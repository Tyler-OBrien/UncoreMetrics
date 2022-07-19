using Microsoft.EntityFrameworkCore;
using Shared_Collectors.Games.Steam.Generic;
using UncoreMetrics.Data;

namespace V_Rising_Collector;

public class Worker : BackgroundService
{
    private const ulong VRisingAppId = 1604030;
    private readonly ILogger<Worker> _logger;

    private readonly IServiceScopeFactory _scopeFactory;

    public const int SECONDS_BETWEEN_DISCOVERY = 600;

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



        if (steamStats == null) throw new InvalidOperationException("Cannot resolve IGenericSteamStats Service");


        if (_nextDiscoveryTime < DateTime.UtcNow)
        {
            Console.WriteLine("----------------------");
            Console.WriteLine("Starting Discovery...");
            Console.WriteLine("----------------------");
            _nextDiscoveryTime = DateTime.UtcNow.AddSeconds(SECONDS_BETWEEN_DISCOVERY);
            var servers = await steamStats.GenericServerDiscovery(VRisingAppId);
            Console.WriteLine("----------------------");
            Console.WriteLine($"Discovery Complete... Found {servers.Count} Servers.");
            Console.WriteLine("----------------------");
        }
        else
        {
            Console.WriteLine("----------------------");
            Console.WriteLine("Starting Poll...");
            Console.WriteLine("----------------------"); 
            var servers = await steamStats.GenericServerPoll(VRisingAppId);
            Console.WriteLine("----------------------");
            Console.WriteLine($"Poll Complete... Found {servers.Count} Servers.");
            Console.WriteLine("----------------------");
        }
    }
}