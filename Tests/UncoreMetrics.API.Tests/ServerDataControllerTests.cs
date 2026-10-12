using System.Net;
using System.Text.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UncoreMetrics.Data.ClickHouse.Models;

namespace UncoreMetrics.API.Tests;

public class ServerDataControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ServerDataControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _factory.ClickHouse.ClearReceivedCalls();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static async Task<JsonElement> ReadJson(HttpResponseMessage response)
    {
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync(Token)).RootElement;
    }

    private static async Task AssertError(HttpResponseMessage response, HttpStatusCode status, string type)
    {
        Assert.Equal(status, response.StatusCode);
        var error = (await ReadJson(response)).GetProperty("error");
        Assert.Equal((int)status, error.GetProperty("code").GetInt32());
        Assert.Equal(type, error.GetProperty("type").GetString());
    }

    [Theory]
    [InlineData("uptimedata")]
    [InlineData("playerdata")]
    public async Task GroupByOfADayOrMore_IsRejected(string endpoint)
    {
        var response = await _client.GetAsync($"/v2/servers/{endpoint}/{Guid.NewGuid()}?groupby=24", Token);

        await AssertError(response, HttpStatusCode.BadRequest, "too_old_data");
    }

    [Theory]
    [InlineData("uptimedata", 0)]
    [InlineData("uptimedata", -1)]
    [InlineData("playerdata", 0)]
    public async Task NonPositiveGroupBy_IsRejected(string endpoint, int groupBy)
    {
        var response = await _client.GetAsync($"/v2/servers/{endpoint}/{Guid.NewGuid()}?groupby={groupBy}", Token);

        await AssertError(response, HttpStatusCode.BadRequest, "invalid_groupby");
    }

    [Theory]
    [InlineData("uptimedata", "?hours=251")]
    [InlineData("uptimedata", "?hours=1000&groupby=1")]
    [InlineData("playerdata", "?hours=251")]
    public async Task TooManyResults_IsRejected(string endpoint, string query)
    {
        var response = await _client.GetAsync($"/v2/servers/{endpoint}/{Guid.NewGuid()}{query}", Token);

        await AssertError(response, HttpStatusCode.BadRequest, "too_many_results");
    }

    [Fact]
    public async Task UptimeData_DefaultsTo24HoursAndReturnsData()
    {
        var id = Guid.NewGuid();
        _factory.ClickHouse.GetUptimeData(id.ToString(), 24, Arg.Any<CancellationToken>())
            .Returns([new ClickHouseUptimeData { ServerId = id, OnlineCount = 3, PingCount = 4 }]);

        var response = await _client.GetAsync($"/v2/servers/uptimedata/{id}", Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = (await ReadJson(response)).GetProperty("data");
        Assert.Equal(1, data.GetArrayLength());
        await _factory.ClickHouse.Received(1).GetUptimeData(id.ToString(), 24, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PlayerData_WithGroupBy_UsesGroupedQuery()
    {
        var id = Guid.NewGuid();
        _factory.ClickHouse.GetPlayerData(id.ToString(), 48, 2, Arg.Any<CancellationToken>()).Returns([]);

        var response = await _client.GetAsync($"/v2/servers/playerdata/{id}?hours=48&groupby=2", Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await _factory.ClickHouse.Received(1).GetPlayerData(id.ToString(), 48, 2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OverallAnalytics_ReturnsCount()
    {
        _factory.ClickHouse.GetOverallAnalytics(Arg.Any<CancellationToken>()).Returns(1234UL);

        var response = await _client.GetAsync("/v2/servers/OverallAnalytics", Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = (await ReadJson(response)).GetProperty("data");
        Assert.Equal(1234UL, data.GetProperty("serverAnalyticsLast24H").GetUInt64());
    }

    [Fact]
    public async Task InvalidRouteGuid_ReturnsModelStateError()
    {
        var response = await _client.GetAsync("/v2/servers/uptimedata/not-a-guid", Token);

        await AssertError(response, HttpStatusCode.BadRequest, "resource_missing");
    }

    [Fact]
    public async Task UnknownRoute_ReturnsJsonNotFound()
    {
        var response = await _client.GetAsync("/v2/does-not-exist", Token);

        await AssertError(response, HttpStatusCode.NotFound, "path_not_found");
    }

    [Fact]
    public async Task WrongMethod_ReturnsJsonMethodNotAllowed()
    {
        var response = await _client.PostAsync("/v2/servers/OverallAnalytics", null, Token);

        await AssertError(response, HttpStatusCode.MethodNotAllowed, "method_not_allowed");
    }

    [Fact]
    public async Task UnhandledException_Returns500JsonError()
    {
        // A per-server endpoint, which isn't output cached, so this can't be answered from another test's cached response
        var id = Guid.NewGuid();
        _factory.ClickHouse.GetUptimeData(id.ToString(), 24, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("ClickHouse is down"));

        var response = await _client.GetAsync($"/v2/servers/uptimedata/{id}", Token);

        await AssertError(response, HttpStatusCode.InternalServerError, "internal_error");
    }
}
