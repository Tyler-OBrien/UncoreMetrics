using Microsoft.EntityFrameworkCore;
using Ping_Collector_Probe.Models;
using Ping_Collector_Probe.Services;
using Polly;
using Polly.Extensions.Http;
using Sentry.Extensions.Logging.Extensions.DependencyInjection;
using Sentry.Extensions.Logging;
using Serilog.Context;
using Serilog;
using Serilog.Events;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.Configuration;
using UncoreMetrics.Data;

namespace Ping_Collector_Probe
{
    public class Program
    {
        private const string outputFormat =
  "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}:{Resolver}:{RunType}] {Message:lj} {Exception}{NewLine}";

    public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
                .WriteTo.Async(config =>
                {
                    config.File($"Logs/Log.log", outputTemplate: outputFormat,
                        restrictedToMinimumLevel: LogEventLevel.Information, retainedFileCountLimit: 10,
                        rollingInterval: RollingInterval.Day);
                    config.Console(outputTemplate: outputFormat, restrictedToMinimumLevel: LogEventLevel.Information);
                }).Enrich.FromLogContext().CreateLogger();
            Log.Logger.Information("Loaded SeriLog Logger");
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            try
            {
                Log.Information("Starting host");
                await BuildHost(args).RunAsync();
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

        public static IHost BuildHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration.GetSection("Base");
                    var baseConfiguration = configuration.Get<ProbeConfiguration>();
                    services.Configure<ProbeConfiguration>(configuration);

                    services.Configure<SentryLoggingOptions>(options =>
                    {
                        options.Dsn = baseConfiguration.SENTRY_DSN;
                        options.SendDefaultPii = true;
                        options.AttachStacktrace = true;
                        options.MinimumBreadcrumbLevel = LogLevel.Debug;
                        options.MinimumEventLevel = LogLevel.Warning;
                        options.TracesSampleRate = 1.0;
                    });
                    services.AddSentry<SentryLoggingOptions>();
                    services.AddLogging();


                    services.AddSingleton<IScrapeJobStatusService, ScrapeJobStatusService>();
                    services.AddScoped<IPingCollectorAPI, PingCollectorAPI>();
                    services.AddHttpClient<IPingCollectorAPI, PingCollectorAPI>()
                        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                        .AddPolicyHandler(GetRetryPolicy());


                    services.AddHostedService<Worker>();
                })
                .UseSerilog()
                .Build();
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromMilliseconds(Math.Max(50, retryAttempt * 50)));
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