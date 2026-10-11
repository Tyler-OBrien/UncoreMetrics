using System.Net;
using UncoreMetrics.Steam_Collector.SteamServers.ServerQuery;
using UncoreMetrics.Steam_Collector.SteamServers.WebAPI;

namespace UncoreMetrics.Tests;

public class SteamServerQueryTests
{
    [Theory]
    [InlineData("1.2.3.4:27015", "1.2.3.4", 27015)]
    [InlineData("[2001:db8::1]:27016", "2001:db8::1", 27016)]
    [InlineData("2001:db8::1:27017", "2001:db8::1", 27017)]
    public void ParseIPAndPort_SplitsOnLastColon(string input, string expectedAddress, int expectedPort)
    {
        var (address, port) = SteamServerQuery.ParseIPAndPort(input);

        Assert.Equal(IPAddress.Parse(expectedAddress), address);
        Assert.Equal(expectedPort, port);
    }

    [Theory]
    [InlineData("1.2.3.4")]
    [InlineData("1.2.3.4:notaport")]
    [InlineData("not.an.ip:27015")]
    public void ParseIPAndPort_ThrowsOnInvalidInput(string input)
    {
        Assert.Throws<InvalidOperationException>(() => SteamServerQuery.ParseIPAndPort(input));
    }

    [Fact]
    public void QueryBuilder_ChainsFiltersInOrder()
    {
        var filter = SteamServerListQueryBuilder.New()
            .AppID("252490")
            .Dedicated()
            .Linux()
            .NotEmpty()
            .NoPassword()
            .Map("Procedural Map")
            .CollapseAddrHash();

        Assert.Equal(
            "\\appid\\252490\\dedicated\\1\\linux\\1\\empty\\1\\password\\0\\map\\Procedural%20Map\\collapse_addr_hash\\1",
            filter.ToString());
    }

    [Fact]
    public void QueryBuilder_EscapesAndJoinsTagLists()
    {
        var filter = SteamServerListQueryBuilder.New()
            .Gametype(["pve", "a b"])
            .GamedataOr("x&y")
            .GameAddr("1.2.3.4:27015");

        Assert.Equal("\\gametype\\pve,a%20b\\gamedataor\\x%26y\\gameaddr\\1.2.3.4%3A27015", filter.ToString());
    }

    [Fact]
    public void QueryBuilder_EmptyBuilderIsEmptyString()
    {
        Assert.Equal(string.Empty, new SteamServerListQueryBuilder().ToString());
    }
}
