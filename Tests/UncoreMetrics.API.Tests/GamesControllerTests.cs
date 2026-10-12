using System.Net;
using System.Text.Json;
using UncoreMetrics.Data.GameData;

namespace UncoreMetrics.API.Tests;

public class GamesControllerTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task GetGames_ReturnsStaticGameList()
    {
        var response = await factory.CreateClient().GetAsync("/v1/games", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var games = json.RootElement.GetProperty("data").EnumerateArray().ToList();
        Assert.Equal(StaticGameInfo.Games.Count, games.Count);
        var rust = games.Single(g => g.GetProperty("appId").GetUInt64() == 252490);
        Assert.Equal("Rust", rust.GetProperty("name").GetString());
        Assert.Equal("Rust", rust.GetProperty("slugName").GetString());
    }

    [Fact]
    public void GameSlugs_AreUrlSafeAndUnique()
    {
        var slugs = StaticGameInfo.Games.Select(g => g.SlugName).ToList();

        Assert.Equal(slugs.Count, slugs.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(slugs, slug => Assert.Matches("^[A-Za-z0-9-]+$", slug));
    }
}
