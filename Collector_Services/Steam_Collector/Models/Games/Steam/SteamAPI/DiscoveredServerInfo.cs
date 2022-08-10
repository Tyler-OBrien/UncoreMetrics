using System.Net;
using Okolni.Source.Common;
using Okolni.Source.Query.Responses;
using UncoreMetrics.Data;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Models.Tools.Maxmind;

namespace UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;

public class DiscoveredServerInfo : IGenericServerInfo
{
    public DiscoveredServerInfo(IPAddress address, int port, SteamListServer server, InfoResponse serverInfo,
        PlayerResponse serverPlayers, RuleResponse? serverRules, IPInformation ipInformation)
    {
        Address = address;
        Port = port;
        Server = server;
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
        ExistingServer.Map = ServerInfo.Map;
        ExistingServer.Keywords = ServerInfo.KeyWords;
        ExistingServer.VAC = ServerInfo.VAC;
        ExistingServer.Visibility = ServerInfo.Visibility == Enums.Visibility.Private ? true : false;
        ExistingServer.Environment = ServerInfo.Environment.ResolveEnvironment();
        ExistingServer.SteamID = ServerInfo.SteamID;
        ExistingServer.ASN = IpInformation?.AutonomousSystemNumber;
        if (IpInformation != null && Enum.TryParse(IpInformation.ContinentCode, true, out Continent continent))
            ExistingServer.Continent = continent;
        ExistingServer.Country = IpInformation?.CountryCodeISO;
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