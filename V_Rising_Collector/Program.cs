using Shared_Collectors;
using Shared_Collectors.Games.Steam.Generic;
using Shared_Collectors.Models;
using V_Rising_Collector;

namespace Company.WebApplication1;

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext,services) =>
            {
                IConfiguration configuration = hostContext.Configuration.GetSection("Application");
                services.Configure<VRisingConfiguration>(configuration);
                services.ConfigureSharedServices(configuration.Get<BaseConfiguration>());
                services.AddSingleton<SteamAPI>();
                services.AddHostedService<Worker>();


            })
            .Build();




        host.Run();
    }
}