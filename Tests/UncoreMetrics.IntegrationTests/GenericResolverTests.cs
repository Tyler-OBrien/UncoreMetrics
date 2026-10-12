using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace UncoreMetrics.IntegrationTests;

/// <summary>Games tracked without a game-specific table, against Postgres with the production schema.</summary>
public class GenericResolverTests(PostgresFixture postgres)
{
    private async Task<long> ServerUpdateCount(ulong appId)
    {
        await using var conn = new NpgsqlConnection(postgres.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("select coalesce(sum(updates), 0) from server_update_counts where app_id = @a", conn);
        cmd.Parameters.AddWithValue("a", (decimal)appId);
        return Convert.ToInt64(await cmd.ExecuteScalarAsync());
    }

    private async Task EnsureUpdateCounter()
    {
        // Count row updates on Servers per app id, to check how many times a submit writes each row
        await using var conn = new NpgsqlConnection(postgres.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("""
            create table if not exists server_update_counts (app_id numeric primary key, updates bigint not null);
            create or replace function count_server_updates() returns trigger as $$
            begin
              insert into server_update_counts values (new."AppID", 1)
              on conflict (app_id) do update set updates = server_update_counts.updates + 1;
              return new;
            end $$ language plpgsql;
            drop trigger if exists count_server_updates on "Servers";
            create trigger count_server_updates after update on "Servers" for each row execute function count_server_updates();
            """, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task Submit_PlainServers_WritesServersTableOncePerServer()
    {
        postgres.SkipIfUnavailable();
        await EnsureUpdateCounter();
        var servers = PostgresFixture.CreateServers(100, 730, "Counter-Strike 2", addressOffset: 0);

        await using (var context = postgres.CreateContext())
            await PostgresFixture.CreateGenericResolver(context, "CS2").Submit(servers);
        var afterInsert = await ServerUpdateCount(730);

        foreach (var server in servers) server.Players = 20;
        await using (var context = postgres.CreateContext())
            await PostgresFixture.CreateGenericResolver(context, "CS2").Submit(servers);

        Assert.Equal(100, await ServerUpdateCount(730) - afterInsert);
        await using var verify = postgres.CreateContext();
        var stored = await verify.Servers.AsNoTracking().SingleAsync(s => s.ServerID == servers[0].ServerID, TestContext.Current.CancellationToken);
        Assert.Equal(20u, stored.Players);
        Assert.Equal(servers[0].FoundAt, stored.FoundAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task GetServers_ReturnsOnlyThatGamesDueServers()
    {
        postgres.SkipIfUnavailable();
        var valheim = PostgresFixture.CreateServers(5, 892970, "Valheim", addressOffset: 10_000);
        var l4d2 = PostgresFixture.CreateServers(5, 550, "Left 4 Dead 2", addressOffset: 20_000);
        valheim[0].NextCheck = DateTime.UtcNow.AddMinutes(5); // not due yet

        await using (var context = postgres.CreateContext())
        {
            await PostgresFixture.CreateGenericResolver(context, "Valheim").Submit(valheim);
            await PostgresFixture.CreateGenericResolver(context, "L4D2").Submit(l4d2);
        }

        await using var read = postgres.CreateContext();
        var due = await PostgresFixture.CreateGenericResolver(read, "Valheim").GetServers();

        // 4 due Valheim servers, polled even though that's below the batch size, because they're 10 minutes overdue
        Assert.Equal(4, due.Count);
        Assert.All(due, s => Assert.Equal(892970UL, s.AppID));
    }
}
