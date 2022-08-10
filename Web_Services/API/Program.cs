using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;
using Serilog.Events;
using UncoreMetrics.API.Middleware;
using UncoreMetrics.API.Models;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.Configuration;

namespace UncoreMetrics.API;

public class Program
{
    public static int Main(string[] args)
    {
        const string outputFormat =
            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj} {Exception}{NewLine}";

        Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
#if !DEBUG
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .WriteTo.Async(config =>
            {
                config.File("Logs/Log.log", outputTemplate: outputFormat,
                    restrictedToMinimumLevel: LogEventLevel.Information, retainedFileCountLimit: 30,
                    rollingInterval: RollingInterval.Day);
                config.Console(outputTemplate: outputFormat, restrictedToMinimumLevel: LogEventLevel.Information);
            }).Enrich.FromLogContext().CreateLogger();
        Log.Logger.Information("Loaded SeriLog Logger");

        try
        {
            Log.Information("Starting host");
            BuildHost(args).Run();
            return 0;
        }
        // Note: This will change in .net 7 https://github.com/dotnet/runtime/issues/60600#issuecomment-1068323222
        // This is being done so we don't report errors about the Host being quit/exited.
        catch (Exception e) when (e is not OperationCanceledException && e.GetType().Name != "StopTheHostException")
        {
            Log.Fatal(e, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static WebApplication BuildHost(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        IConfiguration configuration = builder.Configuration.GetSection("Base");
        var apiConfiguration = configuration.Get<APIConfiguration>();
        builder.Services.Configure<BaseConfiguration>(configuration);
        builder.Services.Configure<APIConfiguration>(configuration);


        builder.Host.UseSerilog();
#if !DEBUG // Annoying to get Sentry errors from Dev Env
            if (string.IsNullOrWhiteSpace(apiConfiguration.SENTRY_DSN) == false)
                builder.WebHost.UseSentry(options =>
                {
                    options.Dsn = apiConfiguration.SENTRY_DSN;
                    options.SendDefaultPii = true;
                    options.AttachStacktrace = true;
                    options.MaxRequestBodySize = RequestSize.Always;
                    options.MinimumBreadcrumbLevel = LogLevel.Debug;
                    options.MinimumEventLevel = LogLevel.Warning;
                });
#endif


        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        builder.WebHost.UseKestrel(options => { options.AddServerHeader = false; });

        builder.Services.AddDbContext<ServersContext>(options =>
        {
            options.UseNpgsql(apiConfiguration.PostgresConnectionString);
        });
        builder.Services.AddScoped<IClickHouseService, ClickHouseService>();

        builder.Services.AddScoped<JSONErrorMiddleware>();
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = ctx => new ModelStateFilterJSON();
        });


        var app = builder.Build();


        if (apiConfiguration.Prometheus_Metrics_Port != default)
        {
            Log.Logger.Information($"Enabling Prometheus Metrics at port {apiConfiguration.Prometheus_Metrics_Port}.");
            app.UseMetricServer(apiConfiguration.Prometheus_Metrics_Port);
        }

        app.UseSwagger();
        app.UseSwaggerUI();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();


        app.UseMiddleware<JSONErrorMiddleware>();

        app.UseAuthorization();

        app.MapControllers();


        if (apiConfiguration.Prometheus_Metrics_Port != default) app.UseHttpMetrics();

        return app;
    }
}