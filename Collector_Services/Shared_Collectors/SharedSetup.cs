using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared_Collectors.Games.Steam.Generic;
using Shared_Collectors.Games.Steam.Generic.WebAPI;
using Shared_Collectors.Models;
using Shared_Collectors.Tools.Maxmind;
using UncoreMetrics.Data;

namespace Shared_Collectors;

public static class SharedSetup
{
    public static void ConfigureSharedServices(this IServiceCollection services, HostBuilderContext hostContext)
    {
        IConfiguration configuration = hostContext.Configuration.GetSection("Base");
        var baseConfiguration = configuration.Get<BaseConfiguration>();
        services.Configure<BaseConfiguration>(configuration);
        services.AddSingleton<ISteamAPI, SteamAPI>();
        services.AddDbContext<ServersContext>(options =>
            options.UseNpgsql(baseConfiguration.PostgresConnectionString));

        services.AddSingleton<IGeoIPService, MaxMindService>();
        services.AddScoped<ISteamServers, SteamServers>();


        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
    }

    private static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Console.WriteLine($"[ERROR] Unobserved Error: {e} - {e.Exception} - {sender}");
        throw e.Exception;
    }
}