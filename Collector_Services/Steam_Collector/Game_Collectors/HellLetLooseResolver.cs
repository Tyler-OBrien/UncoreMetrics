﻿using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.HellLetLoose;

namespace Steam_Collector.Game_Collectors;

public class HellLetLooseResolver : BaseResolver
{
    public HellLetLooseResolver(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers) :
        base(baseConfiguration, serversContext, steamServers)
    {
    }


    public override string Name => "HellLetLoose";
    public override ulong AppId => 686810;


    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var customServers = new List<HellLetLooseServer>(servers.Select(ResolveServerDetails));
        await Submit(customServers);
    }

    private HellLetLooseServer ResolveServerDetails(IGenericServerInfo server)
    {
        HellLetLooseServer customServer = new();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);

        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return customServer;
    }
}