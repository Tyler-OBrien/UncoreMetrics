using Shared_Collectors;

namespace V_Rising_Collector;

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