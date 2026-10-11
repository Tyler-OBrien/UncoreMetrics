using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using UncoreMetrics.Data;

namespace UncoreMetrics.API.Tests;

public class GenericServerControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private static readonly Guid PopularServerId = Guid.NewGuid();
    private static readonly Guid QuietServerId = Guid.NewGuid();
    private static readonly Guid DeadServerId = Guid.NewGuid();

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public GenericServerControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServersContext>();
        if (context.Servers.Any())
            return;

        await _factory.SeedAsync(
            CreateServer(PopularServerId, "Popular", 90, dead: false),
            CreateServer(QuietServerId, "Quiet", 5, dead: false),
            CreateServer(DeadServerId, "Dead", 0, dead: true));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static Server CreateServer(Guid id, string name, uint players, bool dead)
    {
        var address = IPAddress.Parse("1.2.3.4");
        return new Server
        {
            ServerID = id,
            Name = name,
            Game = "Rust",
            Map = "Procedural Map",
            AppID = 252490,
            Address = address,
            IpAddressBytes = address.GetAddressBytes(),
            Port = 28015,
            QueryPort = 28016,
            Players = players,
            MaxPlayers = 100,
            IsOnline = !dead,
            ServerDead = dead,
            LastCheck = DateTime.UtcNow,
            NextCheck = DateTime.UtcNow,
            FoundAt = DateTime.UtcNow
        };
    }

    private static async Task<JsonElement> ReadJson(HttpResponseMessage response)
    {
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync(Token)).RootElement;
    }

    [Fact]
    public async Task GetServers_ExcludesDeadAndOrdersByPlayers()
    {
        var response = await _client.GetAsync("/v1/servers", Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = (await ReadJson(response)).GetProperty("data");
        var names = data.GetProperty("results").EnumerateArray().Select(s => s.GetProperty("name").GetString());
        Assert.Equal(["Popular", "Quiet"], names);
        Assert.Equal(2, data.GetProperty("rowCount").GetInt32());
    }

    [Fact]
    public async Task GetServers_PagesResults()
    {
        var response = await _client.GetAsync("/v1/servers?page=2&pageSize=1", Token);

        var data = (await ReadJson(response)).GetProperty("data");
        var result = Assert.Single(data.GetProperty("results").EnumerateArray());
        Assert.Equal("Quiet", result.GetProperty("name").GetString());
        Assert.Equal(2, data.GetProperty("currentPage").GetInt32());
        Assert.Equal(2, data.GetProperty("pageCount").GetInt32());
    }

    [Theory]
    [InlineData("?pageSize=101", "page_size_out_of_range")]
    [InlineData("?page=0", "page_out_of_range")]
    public async Task GetServers_RejectsInvalidPaging(string query, string errorType)
    {
        var response = await _client.GetAsync($"/v1/servers{query}", Token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(errorType, (await ReadJson(response)).GetProperty("error").GetProperty("type").GetString());
    }

    [Fact]
    public async Task GetServer_ReturnsFullServer()
    {
        var response = await _client.GetAsync($"/v1/servers/{PopularServerId}", Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = (await ReadJson(response)).GetProperty("data");
        Assert.Equal("Popular", data.GetProperty("name").GetString());
        Assert.Equal("1.2.3.4", data.GetProperty("ipAddress").GetString());
    }

    [Fact]
    public async Task GetServer_UnknownIdReturnsNoData()
    {
        var response = await _client.GetAsync($"/v1/servers/{Guid.NewGuid()}", Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False((await ReadJson(response)).TryGetProperty("data", out _));
    }
}
