using System.Net;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace UncoreMetrics.API.Tests;

/// <summary>Each test gets its own factory, so its cache starts empty.</summary>
public class OutputCacheTests : IAsyncLifetime
{
    private readonly ApiFactory _factory = new();
    private HttpClient _client = null!;

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync()
    {
        _client = _factory.CreateClient();
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task OverallAnalytics_IsServedFromCacheOnRepeat()
    {
        _factory.ClickHouse.GetOverallAnalytics(Arg.Any<CancellationToken>()).Returns(1UL, 2UL);

        var first = await _client.GetStringAsync("/v2/servers/OverallAnalytics", Token);
        var second = await _client.GetStringAsync("/v2/servers/OverallAnalytics", Token);

        Assert.Equal(first, second);
        await _factory.ClickHouse.Received(1).GetOverallAnalytics(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DifferentQueryStrings_AreCachedSeparately()
    {
        _factory.ClickHouse.GetPlayerDataOverall(Arg.Any<ulong?>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);

        await _client.GetAsync("/v1/servers/playerdataoverall/252490?hours=24", Token);
        await _client.GetAsync("/v1/servers/playerdataoverall/393380?hours=24", Token);
        await _client.GetAsync("/v1/servers/playerdataoverall/252490?hours=24", Token);

        await _factory.ClickHouse.Received(1).GetPlayerDataOverall(252490UL, 24, Arg.Any<CancellationToken>());
        await _factory.ClickHouse.Received(1).GetPlayerDataOverall(393380UL, 24, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PerServerData_IsNotCached()
    {
        var id = Guid.NewGuid();
        _factory.ClickHouse.GetUptimeData(id.ToString(), 24, Arg.Any<CancellationToken>()).Returns([]);

        await _client.GetAsync($"/v2/servers/uptimedata/{id}", Token);
        await _client.GetAsync($"/v2/servers/uptimedata/{id}", Token);

        await _factory.ClickHouse.Received(2).GetUptimeData(id.ToString(), 24, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Errors_AreNotCached()
    {
        _factory.ClickHouse.GetOverallAnalytics(Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("down"), _ => Task.FromResult(5UL));

        var failed = await _client.GetAsync("/v2/servers/OverallAnalytics", Token);
        var recovered = await _client.GetAsync("/v2/servers/OverallAnalytics", Token);

        Assert.Equal(HttpStatusCode.InternalServerError, failed.StatusCode);
        Assert.Equal(HttpStatusCode.OK, recovered.StatusCode);
    }
}
