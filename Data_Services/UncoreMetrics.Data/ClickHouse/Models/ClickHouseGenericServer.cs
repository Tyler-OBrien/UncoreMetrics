using System.Net;
using System.Net.Sockets;

namespace UncoreMetrics.Data.ClickHouse.Models;

public class ClickHouseGenericServer
{
    public ClickHouseGenericServer()
    {
    }

    public ClickHouseGenericServer(Guid serverId, ulong appId, IPAddress addressIPv4, IPAddress addressIPv6,
        ushort port, ushort queryPort, uint players, uint maxPlayers, ushort retriesUsed, bool visibility,
        string environment, bool vac, string keywords, ulong serverSteamId, uint asn, string country,
        Continent continent, bool isOnline, bool isDead, uint failedChecks, DateTime lastCheck, DateTime nextCheck,
        DateTime currentCheckTime)
    {
        ServerID = serverId;
        AppID = appId;
        AddressIPv4 = addressIPv4;
        AddressIPv6 = addressIPv6;
        Port = port;
        QueryPort = queryPort;
        Players = players;
        MaxPlayers = maxPlayers;
        RetriesUsed = retriesUsed;
        Visibility = visibility;
        Environment = environment;
        VAC = vac;
        Keywords = keywords;
        ServerSteamID = serverSteamId;
        ASN = asn;
        Country = country;
        Continent = continent;
        IsOnline = isOnline;
        IsDead = isDead;
        FailedChecks = failedChecks;
        LastCheck = lastCheck;
        NextCheck = nextCheck;
        CurrentCheckTime = currentCheckTime;
    }

    public Guid ServerID { get; set; }

    public ulong AppID { get; set; }

    public IPAddress AddressIPv4 { get; set; }

    public IPAddress AddressIPv6 { get; set; }

    public ushort Port { get; set; }

    public ushort QueryPort { get; set; }

    public uint Players { get; set; }

    public uint MaxPlayers { get; set; }

    public ushort RetriesUsed { get; set; }

    public bool Visibility { get; set; }

    public string Environment { get; set; }

    public bool VAC { get; set; }

    public string Keywords { get; set; }

    public ulong ServerSteamID { get; set; }

    public uint ASN { get; set; }

    public string Country { get; set; }

    public Continent Continent { get; set; }

    public bool IsOnline { get; set; }

    public bool IsDead { get; set; }

    public uint FailedChecks { get; set; }


    public DateTime LastCheck { get; set; }

    public DateTime NextCheck { get; set; }

    public DateTime CurrentCheckTime { get; set; }

    public static IEnumerable<object[]> ToDatabase(IEnumerable<ClickHouseGenericServer> genericServers)
    {
        return genericServers.Select(server => new object[]
        {
            server.ServerID, server.AppID, server.AddressIPv4, server.AddressIPv6, server.Port, server.QueryPort,
            server.Players, server.MaxPlayers, server.RetriesUsed, server.Visibility, server.Environment, server.VAC,
            server.Keywords, server.ServerSteamID, server.ASN, server.Country, server.Continent, server.IsOnline,
            server.IsDead, server.FailedChecks, server.LastCheck, server.NextCheck, server.CurrentCheckTime
        });
    }

    /// <summary>
    ///     Converts
    ///     <param name="server"></param>
    ///     to <see cref="ClickHouseGenericServer" />
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    // This is messy because our Maxmind library and Query Pool library are trying to remain CLS complaint by not using unsigned numbers, and our Server model is Postgres which doesn't implement unsigned numbers
    public static ClickHouseGenericServer FromServer(Server server)
    {
        return new(server.ServerID, server.AppID,
            ReturnIPv4Address(server.Address),
            ReturnIPv6Address(server.Address),
            (ushort)server.Port, (ushort)server.QueryPort, server.Players, server.MaxPlayers, server.RetriesUsed,
            server.Visibility ?? false, server.Environment.ToString() ?? string.Empty,
            server.VAC ?? false, server.Keywords ?? string.Empty, server.SteamID ?? ulong.MinValue,
            server.ASN.HasValue ? (uint)server.ASN : 0, server.Country ?? string.Empty,
            server.Continent ?? Continent.Unknown,
            server.IsOnline, server.ServerDead, (uint)server.FailedChecks, server.LastCheck, server.NextCheck,
            DateTime.UtcNow);
    }

    private static IPAddress ReturnIPv4Address(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
            return address.MapToIPv4();
        if (address.AddressFamily == AddressFamily.InterNetworkV6)
            return IPAddress.None;
        return address;
    }

    private static IPAddress ReturnIPv6Address(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6 || address.AddressFamily == AddressFamily.InterNetwork)
            return IPAddress.IPv6None;
        return address;
    }
}