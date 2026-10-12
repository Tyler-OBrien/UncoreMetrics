using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Testcontainers.PostgreSql;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.Rust;
using UncoreMetrics.Steam_Collector.Game_Collectors;
using UncoreMetrics.Steam_Collector.Helpers.QueueHelper;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.SteamServers;

[assembly: AssemblyFixture(typeof(UncoreMetrics.IntegrationTests.PostgresFixture))]

namespace UncoreMetrics.IntegrationTests;

/// <summary>
///     One Postgres 17 container for the whole test run, with the real ServersContext migrations applied.
///     When Docker isn't available, <see cref="SkipIfUnavailable" /> skips the tests instead of failing them.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private string? _unavailableReason;

    public string ConnectionString => _container?.GetConnectionString() ?? throw new InvalidOperationException("Postgres isn't running");

    public async ValueTask InitializeAsync()
    {
        try
        {
            _container = new PostgreSqlBuilder("postgres:17").Build();
            await _container.StartAsync();
        }
        catch (Exception ex)
        {
            _unavailableReason = $"Docker isn't available: {ex.GetType().Name}: {ex.Message}";
            _container = null;
            return;
        }

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public void SkipIfUnavailable()
    {
        if (_unavailableReason != null)
            Assert.Skip(_unavailableReason);
    }

    public ServersContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ServersContext>().UseNpgsql(ConnectionString).Options;
        return new ServersContext(options);
    }

    public static SteamCollectorConfiguration Configuration => new()
    {
        GameType = "Rust",
        NodeName = "test",
        ServersPerPollRun = 75000,
        SecondsBetweenChecks = 30,
        SecondsBetweenFailedChecks = [60, 60, 120, 120, 900, 1800, 3600],
        DaysUntilServerMarkedDead = 14,
        PollRunTimeout = 100,
        DiscoveryRunTimeout = 60,
        MaxConcurrency = 2500,
        RunForceTimeoutMinutes = 5
    };

    public static RustResolver CreateRustResolver(ServersContext context) =>
        new(Options.Create(Configuration), context, Substitute.For<ISteamServers>(), Substitute.For<IClickHouseService>(),
            NullLogger<RustResolver>.Instance, Substitute.For<IServerUpdateQueue>());

    /// <summary>Rust servers with unique addresses, all due for a check.</summary>
    public static List<RustServer> CreateRustServers(int count, int addressOffset = 0)
    {
        var now = DateTime.UtcNow;
        return Enumerable.Range(addressOffset, count).Select(i =>
        {
            var address = new IPAddress([(byte)(10 + i / 65536), (byte)(i / 256 % 256), (byte)(i % 256), 1]);
            return new RustServer
            {
                ServerID = Guid.NewGuid(),
                Name = $"Rust server {i} | weekly wipe | 2x",
                Game = "Rust",
                Map = "Procedural Map",
                AppID = 252490,
                Address = address,
                IpAddressBytes = address.GetAddressBytes(),
                Port = 28015,
                QueryPort = 28016,
                Players = (uint)(i % 200),
                MaxPlayers = 200,
                IsOnline = true,
                LastCheck = now.AddMinutes(-1),
                NextCheck = now.AddSeconds(-30),
                FoundAt = now.AddDays(-30),
                Country = "US",
                Continent = Continent.NA,
                ISP = "Example Hosting",
                ASN = 64500,
                Description = "Welcome to the server",
                EntityCount = 120000,
                FPS = 60,
                WorldSize = 4500,
                WorldSeed = i,
                PvE = false
            };
        }).ToList();
    }

    public async ValueTask DisposeAsync()
    {
        if (_container != null)
            await _container.DisposeAsync();
    }
}
