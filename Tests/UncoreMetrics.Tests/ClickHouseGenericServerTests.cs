using System.Net;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse.Models;

namespace UncoreMetrics.Tests;

public class ClickHouseGenericServerTests
{
    private static Server CreateServer(string address)
    {
        return new Server
        {
            ServerID = Guid.NewGuid(),
            Name = "Test",
            Game = "Rust",
            AppID = 252490,
            Address = IPAddress.Parse(address),
            Port = 28015,
            QueryPort = 28016,
            Players = 10,
            MaxPlayers = 100,
            IsOnline = true,
            FailedChecks = 2,
            LastCheck = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            NextCheck = new DateTime(2026, 1, 1, 0, 5, 0, DateTimeKind.Utc)
        };
    }

    [Fact]
    public void FromServer_IPv4_FillsIPv4AndLeavesIPv6Empty()
    {
        var result = ClickHouseGenericServer.FromServer(CreateServer("1.2.3.4"));

        Assert.Equal(IPAddress.Parse("1.2.3.4"), result.AddressIPv4);
        Assert.Equal(IPAddress.IPv6None, result.AddressIPv6);
    }

    [Fact]
    public void FromServer_IPv4MappedToIPv6_IsTreatedAsIPv4()
    {
        var result = ClickHouseGenericServer.FromServer(CreateServer("::ffff:1.2.3.4"));

        Assert.Equal(IPAddress.Parse("1.2.3.4"), result.AddressIPv4);
        Assert.Equal(IPAddress.IPv6None, result.AddressIPv6);
    }

    [Fact]
    public void FromServer_IPv6_FillsIPv6AndLeavesIPv4Empty()
    {
        var result = ClickHouseGenericServer.FromServer(CreateServer("2001:db8::1"));

        Assert.Equal(IPAddress.None, result.AddressIPv4);
        Assert.Equal(IPAddress.Parse("2001:db8::1"), result.AddressIPv6);
    }

    [Fact]
    public void FromServer_DefaultsNullableFields()
    {
        var server = CreateServer("1.2.3.4");

        var result = ClickHouseGenericServer.FromServer(server);

        Assert.Equal(server.ServerID, result.ServerID);
        Assert.Equal((ushort)28015, result.Port);
        Assert.Equal((ushort)28016, result.QueryPort);
        Assert.False(result.Visibility);
        Assert.False(result.VAC);
        Assert.Equal(string.Empty, result.Environment);
        Assert.Equal(string.Empty, result.Keywords);
        Assert.Equal(string.Empty, result.Country);
        Assert.Equal(0UL, result.ServerSteamID);
        Assert.Equal(0U, result.ASN);
        Assert.Equal(Continent.Unknown, result.Continent);
        Assert.Equal(2U, result.FailedChecks);
    }

    [Fact]
    public void FromServer_CopiesSetOptionalFields()
    {
        var server = CreateServer("1.2.3.4");
        server.Environment = 'l';
        server.VAC = true;
        server.ASN = 13335;
        server.Country = "US";
        server.Continent = Continent.NA;

        var result = ClickHouseGenericServer.FromServer(server);

        Assert.Equal("l", result.Environment);
        Assert.True(result.VAC);
        Assert.Equal(13335U, result.ASN);
        Assert.Equal("US", result.Country);
        Assert.Equal(Continent.NA, result.Continent);
    }

    [Fact]
    public void ToDatabase_EmitsColumnsInTableOrder()
    {
        var server = ClickHouseGenericServer.FromServer(CreateServer("1.2.3.4"));

        var row = Assert.Single(ClickHouseGenericServer.ToDatabase([server]));

        Assert.Equal(23, row.Length);
        Assert.Equal(server.ServerID, row[0]);
        Assert.Equal(server.AppID, row[1]);
        Assert.Equal(server.AddressIPv4, row[2]);
        Assert.Equal(server.Players, row[6]);
        Assert.Equal(server.IsOnline, row[17]);
        Assert.Equal(server.CurrentCheckTime, row[22]);
    }
}
