using Microsoft.Extensions.Options;
using UncoreMetrics.Steam_Collector.Game_Collectors;
using UncoreMetrics.Steam_Collector.Models;

namespace UncoreMetrics.Steam_Collector;

public class Worker : BackgroundService
{

    private readonly Random random = new Random();
    public const int SECONDS_BETWEEN_DISCOVERY = 300;

    private readonly SteamCollectorConfiguration _configuration;
    private readonly ILogger<Worker> _logger;

    private readonly IServiceScopeFactory _scopeFactory;

    private DateTime _nextDiscoveryTime = DateTime.UnixEpoch;


    public Worker(ILogger<Worker> logger, IServiceScopeFactory steamStats,
        IOptions<SteamCollectorConfiguration> baseConfiguration)
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
            _logger.LogInformation("Finished Run...");
            await Task.Delay(4000 + random.Next(1000, 4000) , stoppingToken);
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
            _nextDiscoveryTime = DateTime.UtcNow.AddSeconds(SECONDS_BETWEEN_DISCOVERY + random.Next(600));
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