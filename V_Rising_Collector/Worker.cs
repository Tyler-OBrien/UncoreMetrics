using Microsoft.EntityFrameworkCore;
using Shared_Collectors.Games.Steam.Generic;
using UncoreMetrics.Data;

namespace V_Rising_Collector;

public class Worker : BackgroundService
{
    private const string VRisingAppId = "1604030";
    private readonly ILogger<Worker> _logger;

    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory steamStats)
    {
        _logger = logger;
        _scopeFactory = steamStats;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await RunActions();
            //await AltRun();
            await Task.Delay(10000000, stoppingToken);
        }
    }


    private async Task RunActions()
    {
        /*
            var runWith10 = await GetAllServers(10);
            Console.WriteLine($"10 Concurrency, {runWith10.Item1} Players, {runWith10.Item2}% Success");
            var runWith100 = await GetAllServers(100);
            Console.WriteLine($"100 Concurrency, {runWith100.Item1} Players, {runWith100.Item2}% Success");
            var runWith256 = await GetAllServers(256);
            Console.WriteLine($"256 Concurrency, {runWith256.Item1} Players, {runWith256.Item2}% Success");
            var runWith512 = await GetAllServers(512);
            Console.WriteLine($"512 Concurrency, {runWith512.Item1} Players, {runWith512.Item2}% Success");
            var runWith1024 = await GetAllServers(1024);
            Console.WriteLine($"1024 Concurrency, {runWith1024.Item1} Players, {runWith1024.Item2}% Success");
            var runWithUnlimited = await GetAllServers(int.MaxValue);
            Console.WriteLine(
                $"Unlimited Concurrency, {runWithUnlimited.Item1} Players, {runWithUnlimited.Item2}% Success");

            Console.WriteLine(String.Join('-', Enumerable.Repeat("-", 16)));
            Console.WriteLine($"10 Concurrency, {runWith10.Item1} Players, {runWith10.Item2}% Success - Took {runWith10.Item3}ms");
            Console.WriteLine($"100 Concurrency, {runWith100.Item1} Players, {runWith100.Item2}% Success - Took {runWith100.Item3}ms ");
            Console.WriteLine($"256 Concurrency, {runWith256.Item1} Players, {runWith256.Item2}% Success - Took {runWith256.Item3}ms");
            Console.WriteLine($"512 Concurrency, {runWith512.Item1} Players, {runWith512.Item2}% Success - Took {runWith512.Item3}ms ");
            Console.WriteLine($"1024 Concurrency, {runWith1024.Item1} Players, {runWith1024.Item2}% Success - Took {runWith1024.Item3}ms");
            Console.WriteLine($"Unlimited Concurrency, {runWithUnlimited.Item1} Players, {runWithUnlimited.Item2}% Success - Took {runWithUnlimited.Item3}ms");
        Console.WriteLine(String.Join('-', Enumerable.Repeat("-", 16)));

         

        Console.ReadKey(true);
        */


        using var scope = _scopeFactory.CreateScope();

        var steamStats = scope.ServiceProvider.GetService<IGenericSteamStats>();

        var databaseContext = scope.ServiceProvider.GetService<GenericServersContext>();

        var item = await databaseContext.Servers.Where(x => x.IsOnline).FirstOrDefaultAsync();
        


        if (steamStats == null) throw new InvalidOperationException("Cannot resolve IGenericSteamStats Service");

        var servers = await steamStats.HandleGeneric(VRisingAppId);
    }
}