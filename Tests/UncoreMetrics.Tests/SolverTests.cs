using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Okolni.Source.Query.Responses;
using Okolni.Source.Query.Source;
using UncoreMetrics.Data;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Helpers.Maxmind;
using UncoreMetrics.Steam_Collector.Models.Tools.Maxmind;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers;

namespace UncoreMetrics.Tests;

public class SolverTests
{
    private static Server CreateServer() => new()
    {
        ServerID = Guid.NewGuid(),
        Name = "Test",
        Address = IPAddress.Parse("1.2.3.4"),
        QueryPort = 28016
    };

    private static IQueryConnectionPool PoolWithInfo(InfoResponse? info)
    {
        var pool = Substitute.For<IQueryConnectionPool>();
        if (info == null)
            pool.GetInfoAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>()).ThrowsAsync(new TimeoutException());
        else
            pool.GetInfoAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>()).Returns(info);
        pool.GetRulesAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>())
            .Returns(new RuleResponse { Rules = new Dictionary<string, string>() });
        pool.GetPlayersAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>())
            .Returns(new PlayerResponse { Players = [] });
        return pool;
    }

    [Fact]
    public async Task PollSolver_SkipsRulesAndPlayersWhenInfoFails()
    {
        var server = CreateServer();
        var pool = PoolWithInfo(null);

        var (result, success) = await new PollSolver(NullLogger.Instance).Solve(new QueryPoolItem<Server>(pool, server));

        Assert.False(success);
        Assert.NotNull(result);
        Assert.Same(server, result.ExistingServer);
        Assert.Null(result.ServerInfo);
        await pool.DidNotReceive().GetRulesAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>());
        await pool.DidNotReceive().GetPlayersAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>());
    }

    [Fact]
    public async Task PollSolver_QueriesEverythingWhenInfoSucceeds()
    {
        var server = CreateServer();
        var pool = PoolWithInfo(new InfoResponse { Name = "Up" });

        var (result, success) = await new PollSolver(NullLogger.Instance).Solve(new QueryPoolItem<Server>(pool, server));

        Assert.True(success);
        Assert.Equal("Up", result!.ServerInfo!.Name);
        await pool.Received(1).GetRulesAsync(new IPEndPoint(server.Address, server.QueryPort), Arg.Any<int>());
        await pool.Received(1).GetPlayersAsync(new IPEndPoint(server.Address, server.QueryPort), Arg.Any<int>());
    }

    [Fact]
    public async Task PollSolver_WithRulesOff_SucceedsWithoutQueryingRules()
    {
        var server = CreateServer();
        var pool = PoolWithInfo(new InfoResponse { Name = "Valheim server" });

        var (result, success) = await new PollSolver(NullLogger.Instance, queryRules: false)
            .Solve(new QueryPoolItem<Server>(pool, server));

        Assert.True(success);
        Assert.Null(result!.ServerRules);
        await pool.DidNotReceive().GetRulesAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>());
        await pool.Received(1).GetPlayersAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>());
    }

    [Fact]
    public async Task DiscoverySolver_WithRulesOff_KeepsServersThatHaveNoRules()
    {
        var pool = PoolWithInfo(new InfoResponse { Name = "Valheim server" });
        pool.GetRulesAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>()).ThrowsAsync(new TimeoutException());
        var geoIp = Substitute.For<IGeoIPService>();
        geoIp.GetIpInformation(Arg.Any<string>()).Returns(new IPInformation());

        var (result, success) = await new DiscoverySolver(geoIp, NullLogger.Instance, queryRules: false)
            .Solve(new QueryPoolItem<SteamListServer>(pool, new SteamListServer { Address = "1.2.3.4:2457" }));

        Assert.True(success);
        Assert.NotNull(result);
        await pool.DidNotReceive().GetRulesAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>());
    }

    [Fact]
    public async Task DiscoverySolver_SkipsRulesPlayersAndGeoIpWhenInfoFails()
    {
        var pool = PoolWithInfo(null);
        var geoIp = Substitute.For<IGeoIPService>();
        var listServer = new SteamListServer { Address = "1.2.3.4:28016" };

        var (result, success) = await new DiscoverySolver(geoIp, NullLogger.Instance)
            .Solve(new QueryPoolItem<SteamListServer>(pool, listServer));

        Assert.False(success);
        Assert.Null(result);
        await pool.DidNotReceive().GetRulesAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>());
        await pool.DidNotReceive().GetPlayersAsync(Arg.Any<IPEndPoint>(), Arg.Any<int>());
        await geoIp.DidNotReceive().GetIpInformation(Arg.Any<string>());
    }
}
