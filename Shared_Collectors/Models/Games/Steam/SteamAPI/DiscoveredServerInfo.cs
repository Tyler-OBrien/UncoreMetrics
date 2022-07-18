using System.Net;
using Okolni.Source.Query.Responses;
using Shared_Collectors.Models.Tools.Maxmind;
using UncoreMetrics.Data;

namespace Shared_Collectors.Models.Games.Steam.SteamAPI;

public class DiscoveredServerInfo
{
    public DiscoveredServerInfo(IPAddress address, int port, SteamListServer server, InfoResponse serverInfo,
        PlayerResponse serverPlayers, RuleResponse? serverRules, IPInformation ipInformation)
    {
        Address = address;
        Port = port;
        this.server = server;
        this.serverInfo = serverInfo;
        this.serverPlayers = serverPlayers;
        this.serverRules = serverRules;
        IpInformation = ipInformation;
    }



    public SteamListServer server { get; set; }
    public InfoResponse serverInfo { get; set; }

    public PlayerResponse serverPlayers { get; set; }


    public RuleResponse? serverRules { get; set; }

    public IPInformation IpInformation { get; set; }

    public IPAddress Address { get; set; }

    public int Port { get; set; }

    public GenericServer ToGenericServer(int nextCheckSeconds)
    {
        return new GenericServer
        {
            Address = Address,
            IpAddressBytes = Address.GetAddressBytes(),
            QueryPort = Port,
            AppID = server.AppID,
            Game = server.Gamedir,
            ASN = IpInformation?.AutonomousSystemNumber,
            Continent = IpInformation?.Continent,
            Country = IpInformation?.Country,
            Timezone = IpInformation?.TimeZone,
            ISP = IpInformation?.AutonomousSystemOrganization,
            Latitude = IpInformation?.Latitude,
            Longitude = IpInformation?.Longitude,
            IsOnline = true,
            ServerDead = false,
            LastCheck = DateTime.UtcNow,
            MaxPlayers = serverInfo.MaxPlayers,
            Port = serverInfo.Port ?? Port,
            Players =  (uint)serverPlayers.Players.Count,
            FoundAt = DateTime.UtcNow,
            Name = server.Name,
            ServerID = Guid.NewGuid(),
            NextCheck = DateTime.UtcNow.AddSeconds(30),
            FailedChecks = 0
        };
    }
}