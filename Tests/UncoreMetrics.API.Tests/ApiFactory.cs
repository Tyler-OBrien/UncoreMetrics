using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using UncoreMetrics.API;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.Discord;

namespace UncoreMetrics.API.Tests;

/// <summary>
///     Hosts the real API pipeline, with Postgres swapped for EF Core InMemory and ClickHouse replaced by a substitute.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"api-tests-{Guid.NewGuid()}";

    public IClickHouseService ClickHouse { get; } = Substitute.For<IClickHouseService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Base:PostgresConnectionString", "Host=unused");
        builder.UseSetting("Base:ClickhouseConnectionString", "Host=unused");
        // appsettings.json enables a Prometheus listener, which tests don't need (and which would collide across hosts)
        builder.UseSetting("Base:Prometheus_Metrics_Port", "0");

        builder.ConfigureServices(services =>
        {
            RemoveDbContext<ServersContext>(services);
            RemoveDbContext<DiscordContext>(services);
            services.AddDbContext<ServersContext, InMemoryServersContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
            services.AddDbContext<DiscordContext>(options => options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<IClickHouseService>();
            services.AddScoped(_ => ClickHouse);
        });
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        services.RemoveAll<TContext>();
        services.RemoveAll<DbContextOptions<TContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<TContext>>();
    }

    public async Task SeedAsync(params Server[] servers)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServersContext>();
        context.Servers.AddRange(servers);
        await context.SaveChangesAsync();
    }
}
