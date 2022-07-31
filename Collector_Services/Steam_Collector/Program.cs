using Microsoft.EntityFrameworkCore;
using Steam_Collector.Game_Collectors;
using Steam_Collector.Helpers.Maxmind;
using Steam_Collector.Models;
using Steam_Collector.SteamServers;
using Steam_Collector.SteamServers.WebAPI;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.Configuration;

namespace Steam_Collector;

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                IConfiguration configuration = hostContext.Configuration.GetSection("Base");
                var baseConfiguration = configuration.Get<SteamCollectorConfiguration>();
                services.Configure<SteamCollectorConfiguration>(configuration);
                services.Configure<BaseConfiguration>(configuration);

                var resolver = new GameResolvers();
                var gameType = baseConfiguration.GameType;
                var env = Environment.GetEnvironmentVariable("UNCORE_COLLECTOR_GAMETYPE");
                if (string.IsNullOrWhiteSpace(env) == false)
                    gameType = env;

                if (resolver.DoesGameResolverExist(gameType) == false)
                    throw new InvalidOperationException(
                        $"Could not find Game Type Resolver: {baseConfiguration.GameType}, Valid Options: {resolver.GetValidResolvers()}");
                services.AddScoped(typeof(BaseResolver), resolver.GetResolver(gameType));




                services.AddSingleton<ISteamAPI, SteamAPI>();
                services.AddDbContext<ServersContext>(options =>
                {
                    options.UseNpgsql(baseConfiguration.PostgresConnectionString);
                });

                services.AddSingleton<IGeoIPService, MaxMindService>();
                services.AddScoped<ISteamServers, SteamServers.SteamServers>();
                services.AddScoped<IClickHouseService, ClickHouseService>();

                services.AddHostedService<Worker>();
            })
            .Build();

        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        host.Run();
    }
    private static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Console.WriteLine($"[ERROR] Unobserved Error: {e} - {e.Exception} - {sender}");
        throw e.Exception;
    }
}