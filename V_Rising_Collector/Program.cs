using Shared_Collectors;
using V_Rising_Collector;

namespace Company.WebApplication1;

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.ConfigureSharedServices(hostContext);
                services.AddHostedService<Worker>();
            })
            .Build();


        host.Run();
    }
}