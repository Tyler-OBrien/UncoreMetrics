using Microsoft.EntityFrameworkCore;

namespace UncoreMetrics.IntegrationTests;

/// <summary>The collector's real write path (BaseResolver.Submit) against Postgres with the production schema.</summary>
public class SubmitTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Submit_WritesBaseAndGameTables()
    {
        postgres.SkipIfUnavailable();
        var servers = PostgresFixture.CreateRustServers(50, addressOffset: 100_000);

        await using (var context = postgres.CreateContext())
            await PostgresFixture.CreateRustResolver(context).Submit(servers);

        await using var verify = postgres.CreateContext();
        var ids = servers.Select(s => s.ServerID).ToList();
        var stored = await verify.RustServers.AsNoTracking().Where(s => ids.Contains(s.ServerID)).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(50, stored.Count);
        Assert.All(stored, s => Assert.Equal(120000, s.EntityCount));
        Assert.All(stored, s => Assert.Equal("Procedural Map", s.Map));
    }

    [Fact]
    public async Task Submit_Update_KeepsFoundAtAndUpdatesBothTables()
    {
        postgres.SkipIfUnavailable();
        var servers = PostgresFixture.CreateRustServers(20, addressOffset: 200_000);
        var originalFoundAt = servers[0].FoundAt;

        await using (var context = postgres.CreateContext())
            await PostgresFixture.CreateRustResolver(context).Submit(servers);

        // A poll: same servers, new player counts and game data, and a FoundAt that must not overwrite the stored one
        foreach (var server in servers)
        {
            server.Players = 7;
            server.EntityCount = 99;
            server.FoundAt = DateTime.UtcNow;
        }

        await using (var context = postgres.CreateContext())
            await PostgresFixture.CreateRustResolver(context).Submit(servers);

        await using var verify = postgres.CreateContext();
        var stored = await verify.RustServers.AsNoTracking().SingleAsync(s => s.ServerID == servers[0].ServerID, TestContext.Current.CancellationToken);
        Assert.Equal(7u, stored.Players);
        Assert.Equal(99, stored.EntityCount);
        Assert.Equal(originalFoundAt, stored.FoundAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task SearchVector_IsGeneratedByPostgres()
    {
        postgres.SkipIfUnavailable();
        var servers = PostgresFixture.CreateRustServers(1, addressOffset: 300_000);

        await using (var context = postgres.CreateContext())
            await PostgresFixture.CreateRustResolver(context).Submit(servers);

        await using var verify = postgres.CreateContext();
        var found = await verify.Servers.AsNoTracking()
            .AnyAsync(s => s.ServerID == servers[0].ServerID && s.SearchVector.Matches("weekly"), TestContext.Current.CancellationToken);
        Assert.True(found);
    }
}
