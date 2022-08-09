using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Sentry;
using Sentry.Extensibility;
using Sentry.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Steam_Collector.Game_Collectors;
using Steam_Collector.Helpers.Maxmind;
using Steam_Collector.Helpers.ScrapeJobStatus;
using Steam_Collector.Models;
using Steam_Collector.SteamServers;
using Steam_Collector.SteamServers.WebAPI;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.Configuration;

namespace Steam_Collector;

public class Program
{
    private const string outputFormat =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}:{Resolver}:{RunType}] {Message:lj} {Exception}{NewLine}";

    public static int Main(string[] args)
    {
        string extraLogName = "";
        // If env, we assume this is being run as a single program being targetted by mutiple unit files with their own env, so we need custom log names
        var env = Environment.GetEnvironmentVariable("UNCORE_COLLECTOR_GAMETYPE");
        if (env != null)
            extraLogName = $"{env}-";

        Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
#if DEBUG
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
#endif
            .WriteTo.Async(config =>
            {
                config.File($"Logs/{extraLogName}Log.log", outputTemplate: outputFormat,
                    restrictedToMinimumLevel: LogEventLevel.Information, retainedFileCountLimit: 10, rollingInterval: RollingInterval.Day);
                config.Console(outputTemplate: outputFormat, restrictedToMinimumLevel: LogEventLevel.Information);
            }).Enrich.FromLogContext().CreateLogger();
        Log.Logger.Information("Loaded SeriLog Logger");
        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

        try
        {
            Log.Information("Starting host");
            BuildHost(args).Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                
                IConfiguration configuration = hostContext.Configuration.GetSection("Base");
                var baseConfiguration = configuration.Get<SteamCollectorConfiguration>();
                services.Configure<SteamCollectorConfiguration>(configuration);
                services.Configure<BaseConfiguration>(configuration);

                services.Configure<SentryLoggingOptions>(options =>
                {
                    options.Dsn = baseConfiguration.SENTRY_DSN;
                    options.SendDefaultPii = true;
                    options.AttachStacktrace = true;
                    options.MinimumBreadcrumbLevel = LogLevel.Debug;
                    options.MinimumEventLevel = LogLevel.Warning;
                    options.TracesSampleRate = 1.0;
                    options.AddEntityFramework();
                });
                services.AddSentry();
                services.AddLogging();

                var resolver = new GameResolvers();
                var gameType = baseConfiguration.GameType;
                var env = Environment.GetEnvironmentVariable("UNCORE_COLLECTOR_GAMETYPE");
                if (string.IsNullOrWhiteSpace(env) == false)
                {
                    gameType = env;
                    baseConfiguration.GameType = env;
                    services.Configure<SteamCollectorConfiguration>(config => config.GameType = env);
                }

                if (resolver.DoesGameResolverExist(gameType) == false)
                    throw new InvalidOperationException(
                        $"Could not find Game Type Resolver: {baseConfiguration.GameType}, Valid Options: {resolver.GetValidResolvers()}");
                services.AddScoped(typeof(BaseResolver), resolver.GetResolver(gameType));
                LogContext.PushProperty("Resolver", gameType, true);
                Serilog.Log.Logger.Warning("Starting up with Resolver: {gameType}", gameType);
                
                

                services.AddSingleton<ISteamAPI, SteamAPI>();
                services.AddDbContext<ServersContext>(options =>
                {
                    options.UseNpgsql(baseConfiguration.PostgresConnectionString);
                });

                services.AddSingleton<IGeoIPService, MaxMindService>();
                services.AddScoped<ISteamServers, SteamServers.SteamServers>();
                services.AddScoped<IClickHouseService, ClickHouseService>();
                services.AddScoped<IScrapeJobStatusService, ScrapeJobStatusService>();

                services.AddHostedService<Worker>();

            })
            .UseSerilog() // <- Add this line
            .Build();


    private static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Serilog.Log.Logger.Error(e.Exception, "[ERROR] Unobserved Error: {UnobservedTaskExceptionEventArgs} - {UnobservedTaskExceptionEventArgsException} - {senderObj}", e, e.Exception, sender);
        throw e.Exception;
    }

}