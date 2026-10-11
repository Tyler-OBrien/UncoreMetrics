using System.Net;
using UncoreMetrics.Steam_Collector.Helpers.IPAddressExtensions;

namespace UncoreMetrics.Tests;

public class IPAddressExtensionsTests
{
    [Theory]
    [InlineData("10.0.0.1")]
    [InlineData("10.255.255.255")]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.255")]
    [InlineData("192.168.1.1")]
    [InlineData("169.254.10.10")]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    [InlineData("fe80::1")]
    [InlineData("fc00::1")]
    [InlineData("fd12:3456:789a::1")]
    [InlineData("fec0::1")]
    [InlineData("::ffff:192.168.1.1")]
    public void IsPrivate_ReturnsTrueForPrivateRanges(string address)
    {
        Assert.True(IPAddress.Parse(address).IsPrivate());
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("1.1.1.1")]
    [InlineData("172.15.255.255")]
    [InlineData("172.32.0.1")]
    [InlineData("192.169.0.1")]
    [InlineData("2001:4860:4860::8888")]
    [InlineData("::ffff:8.8.8.8")]
    public void IsPrivate_ReturnsFalseForPublicAddresses(string address)
    {
        Assert.False(IPAddress.Parse(address).IsPrivate());
    }
}
