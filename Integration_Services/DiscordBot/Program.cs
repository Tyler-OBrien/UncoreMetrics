using DiscordBot.Helpers;
using Microsoft.EntityFrameworkCore;
using Sentry;
using Sentry.Extensions.Logging;
using Serilog;
using Serilog.Events;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.Configuration;
using UncoreMetrics.Data.Discord;

namespace DiscordBot
{
    public class Program
    {
        private const string outputFormat =
            "[{Timestamp:h:mm:ss ff tt}] [{Level:u3}] [{SourceContext}] {Message:lj} {Exception:j}{NewLine}";

        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
#if !DEBUG
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
#endif
                .WriteTo.Async(config =>
                {
                    config.File("Logs/Log.log", outputTemplate: outputFormat,
                        restrictedToMinimumLevel: LogEventLevel.Information, retainedFileCountLimit: 10,
                        rollingInterval: RollingInterval.Day);
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

            /*
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
                })
                .Build();



            host.Run();
            */
        }

        public static IHost BuildHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration.GetSection("DiscordBotConfig");
                    var baseConfiguration = configuration.Get<UncoreDiscordBotConfiguration>();
                    services.Configure<UncoreDiscordBotConfiguration>(configuration);
                    // Makes it easier to use with DSharpPlus
                    services.Configure<BaseConfiguration>(configuration);
                    if (baseConfiguration == null)
                    {
                        throw new InvalidOperationException("DiscordBotConfig cannot be null, see sample");
                    }

                    if (string.IsNullOrWhiteSpace(baseConfiguration.SENTRY_DSN) == false)
                    {
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
                    }

                    services.AddSentry();
                    services.AddLogging();


                    services.AddDbContext<ServersContext>(options =>
                    {
                        options.UseNpgsql(baseConfiguration.PostgresConnectionString);
                    });
                    services.AddDbContext<DiscordContext>(options =>
                    {
                        options.UseNpgsql(baseConfiguration.PostgresConnectionString);
                    });
                    services.AddScoped<IClickHouseService, ClickHouseService>();
                    services.AddHostedService<Worker>();
                })
                .UseSerilog()
                .Build();
        }

        private static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Logger.Error(e.Exception,
                "[ERROR] Unobserved Error: {UnobservedTaskExceptionEventArgs} - {UnobservedTaskExceptionEventArgsException} - {senderObj}",
                e, e.Exception, sender);
            throw e.Exception;
        }
    }
}