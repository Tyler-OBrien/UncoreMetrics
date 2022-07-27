using System.Net;
using Okolni.Source.Query.Responses;
using Steam_Collector.Models.Tools.Maxmind;
using UncoreMetrics.Data;

namespace Steam_Collector.Models.Games.Steam.SteamAPI;

public class DiscoveredServerInfo : IGenericServerInfo
{
    public DiscoveredServerInfo(IPAddress address, int port, SteamListServer server, InfoResponse serverInfo,
        PlayerResponse serverPlayers, RuleResponse? serverRules, IPInformation ipInformation)
    {
        Address = address;
        Port = port;
        this.Server = server;
        ServerInfo = serverInfo;
        ServerPlayers = serverPlayers;
        ServerRules = serverRules;
        IpInformation = ipInformation;
    }

    public Server? ExistingServer { get; set; }

    public SteamListServer Server { get; set; }

    public IPInformation IpInformation { get; set; }

    public IPAddress Address { get; set; }

    public int Port { get; set; }

    public InfoResponse ServerInfo { get; set; }

    public PlayerResponse ServerPlayers { get; set; }


    public RuleResponse? ServerRules { get; set; }

    internal void UpdateServer(int nextCheckSeconds)
    {
        if (ExistingServer == null) ExistingServer = new Server();

        ExistingServer.Address = Address;
        ExistingServer.IpAddressBytes = Address.GetAddressBytes();
        ExistingServer.QueryPort = Port;
        ExistingServer.AppID = Server.AppID;
        ExistingServer.Game = Server.Gamedir;
        ExistingServer.ASN = IpInformation?.AutonomousSystemNumber;
        ExistingServer.Continent = IpInformation?.Continent;
        ExistingServer.Country = IpInformation?.Country;
        ExistingServer.Timezone = IpInformation?.TimeZone;
        ExistingServer.ISP = IpInformation?.AutonomousSystemOrganization;
        ExistingServer.Latitude = IpInformation?.Latitude;
        ExistingServer.Longitude = IpInformation?.Longitude;
        ExistingServer.IsOnline = true;
        ExistingServer.ServerDead = false;
        ExistingServer.LastCheck = DateTime.UtcNow;
        ExistingServer.MaxPlayers = ServerInfo.MaxPlayers;
        ExistingServer.Port = ServerInfo.Port ?? Port;
        ExistingServer.Players = (uint)ServerPlayers.Players.Count;
        ExistingServer.FoundAt = DateTime.UtcNow;
        ExistingServer.Name = Server.Name;
        ExistingServer.ServerID = Guid.Empty;
        ExistingServer.NextCheck = DateTime.UtcNow.AddSeconds(Random.Shared.Next(30, 90));
        ExistingServer.FailedChecks = 0;
    }
}