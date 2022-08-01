using System.Net;
using Microsoft.Extensions.Options;
using Steam_Collector.Game_Collectors;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.ClickHouse.Models;
using UncoreMetrics.Data.GameData.VRising;

namespace Steam_Collector;

public class Worker : BackgroundService
{

    public const int SECONDS_BETWEEN_DISCOVERY = 600;
    private readonly ILogger<Worker> _logger;

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly SteamCollectorConfiguration _configuration;

    private DateTime _nextDiscoveryTime = DateTime.UnixEpoch;



    public Worker(ILogger<Worker> logger, IServiceScopeFactory steamStats, IOptions<SteamCollectorConfiguration> baseConfiguration)
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
            _logger.LogInformation("Finished Run...");
            await Task.Delay(30000, stoppingToken);
        }
    }


    private async Task RunActions()
    {
        using var scope = _scopeFactory.CreateScope();

        var resolver = scope.ServiceProvider.GetRequiredService<BaseResolver>();


        if (_nextDiscoveryTime < DateTime.UtcNow)
        {
            _logger.LogInformation("----------------------");
            _logger.LogInformation("Starting Discovery...");
            _logger.LogInformation("----------------------");
            _nextDiscoveryTime = DateTime.UtcNow.AddSeconds(SECONDS_BETWEEN_DISCOVERY);
            var serversCount = await resolver.Discovery();
            _logger.LogInformation("----------------------");
            _logger.LogInformation("Discovery Complete... Found {serversCount} Servers.", serversCount);
            _logger.LogInformation("----------------------");
        }
        else
        {
            _logger.LogInformation("----------------------");
            _logger.LogInformation("Starting Poll...");
            _logger.LogInformation("----------------------");
            var serversCount = await resolver.Poll();
            _logger.LogInformation("----------------------");
            _logger.LogInformation("Poll Complete... Reached {serversCount} Servers.", serversCount);
            _logger.LogInformation("----------------------");
        }
    }

}