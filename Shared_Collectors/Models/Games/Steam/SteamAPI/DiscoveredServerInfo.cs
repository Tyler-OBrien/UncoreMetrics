using System.Net;
using Okolni.Source.Query.Responses;
using Shared_Collectors.Models.Tools.Maxmind;
using UncoreMetrics.Data;

namespace Shared_Collectors.Models.Games.Steam.SteamAPI;

public class DiscoveredServerInfo<T> : IGenericServerInfo<T> where T : GenericServer, new()
{
    public DiscoveredServerInfo(IPAddress address, int port, SteamListServer server, InfoResponse serverInfo,
        PlayerResponse serverPlayers, RuleResponse? serverRules, IPInformation ipInformation)
    {
        Address = address;
        Port = port;
        this.server = server;
        ServerInfo = serverInfo;
        ServerPlayers = serverPlayers;
        ServerRules = serverRules;
        IpInformation = ipInformation;
    }

    public SteamListServer server { get; set; }

    public IPInformation IpInformation { get; set; }

    public IPAddress Address { get; set; }

    public int Port { get; set; }

    public T CustomServerInfo { get; set; }
    public InfoResponse ServerInfo { get; set; }

    public PlayerResponse ServerPlayers { get; set; }


    public RuleResponse? ServerRules { get; set; }

    internal T CreateCustomServerInfo(int nextCheckSeconds)
    {
        if (CustomServerInfo == null) CustomServerInfo = new T();


        CustomServerInfo.Address = Address;
        CustomServerInfo.IpAddressBytes = Address.GetAddressBytes();
        CustomServerInfo.QueryPort = Port;
        CustomServerInfo.AppID = server.AppID;
        CustomServerInfo.Game = server.Gamedir;
        CustomServerInfo.ASN = IpInformation?.AutonomousSystemNumber;
        CustomServerInfo.Continent = IpInformation?.Continent;
        CustomServerInfo.Country = IpInformation?.Country;
        CustomServerInfo.Timezone = IpInformation?.TimeZone;
        CustomServerInfo.ISP = IpInformation?.AutonomousSystemOrganization;
        CustomServerInfo.Latitude = IpInformation?.Latitude;
        CustomServerInfo.Longitude = IpInformation?.Longitude;
        CustomServerInfo.IsOnline = true;
        CustomServerInfo.ServerDead = false;
        CustomServerInfo.LastCheck = DateTime.UtcNow;
        CustomServerInfo.MaxPlayers = ServerInfo.MaxPlayers;
        CustomServerInfo.Port = ServerInfo.Port ?? Port;
        CustomServerInfo.Players = (uint)ServerPlayers.Players.Count;
        CustomServerInfo.FoundAt = DateTime.UtcNow;
        CustomServerInfo.Name = server.Name;
        CustomServerInfo.ServerID = Guid.NewGuid();
        CustomServerInfo.NextCheck = DateTime.UtcNow.AddSeconds(Random.Shared.Next(30, 90));
        CustomServerInfo.FailedChecks = 0;
        return CustomServerInfo;
    }
}