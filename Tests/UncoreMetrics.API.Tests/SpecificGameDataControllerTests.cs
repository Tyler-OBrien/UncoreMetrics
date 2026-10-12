using System.Net;
using System.Text.Json;

namespace UncoreMetrics.API.Tests;

public class SpecificGameDataControllerTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task GameWithoutItsOwnTable_ReturnsEmptyData()
    {
        var response = await factory.CreateClient().GetAsync($"/v1/serverdata/{Guid.NewGuid()}/730", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).RootElement.GetProperty("data");
        Assert.Equal(JsonValueKind.Object, data.ValueKind);
        Assert.Empty(data.EnumerateObject());
    }

    [Fact]
    public async Task UnknownGame_IsStillAnError()
    {
        var response = await factory.CreateClient().GetAsync($"/v1/serverdata/{Guid.NewGuid()}/123", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
