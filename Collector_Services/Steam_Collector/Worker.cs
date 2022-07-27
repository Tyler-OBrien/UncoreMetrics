using Microsoft.Extensions.Options;
using Steam_Collector.Game_Collectors;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.VRising;

namespace Steam_Collector;

public class Worker : BackgroundService
{
    private const ulong VRisingAppId = 1604030;

    public const int SECONDS_BETWEEN_DISCOVERY = 600;
    private readonly ILogger<Worker> _logger;

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly BaseConfiguration _configuration;

    private DateTime _nextDiscoveryTime = DateTime.UnixEpoch;



    public Worker(ILogger<Worker> logger, IServiceScopeFactory steamStats, IOptions<BaseConfiguration> baseConfiguration)
    {
        _logger = logger;
        _scopeFactory = steamStats;
        _configuration = baseConfiguration.Value;
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

        var resolver = scope.ServiceProvider.GetRequiredService<BaseResolver>();


        if (_nextDiscoveryTime < DateTime.UtcNow)
        {
            Console.WriteLine("----------------------");
            Console.WriteLine("Starting Discovery...");
            Console.WriteLine("----------------------");
            _nextDiscoveryTime = DateTime.UtcNow.AddSeconds(SECONDS_BETWEEN_DISCOVERY);
            var serversCount = await resolver.Discovery();
            Console.WriteLine("----------------------");
            Console.WriteLine($"Discovery Complete... Found {serversCount} Servers.");
            Console.WriteLine("----------------------");
        }
        else
        {
            Console.WriteLine("----------------------");
            Console.WriteLine("Starting Poll...");
            Console.WriteLine("----------------------");
            var serversCount = await resolver.Poll();
            Console.WriteLine("----------------------");
            Console.WriteLine($"Poll Complete... Reached {serversCount} Servers.");
            Console.WriteLine("----------------------");
        }
    }

}