using Rust_Collector;
using Shared_Collectors;

namespace Company.WebApplication1
{
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
}