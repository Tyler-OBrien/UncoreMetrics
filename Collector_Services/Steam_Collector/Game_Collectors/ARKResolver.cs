﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.ARK;

namespace Steam_Collector.Game_Collectors;

public class ARKResolver : BaseResolver
{
    private readonly ServersContext _genericServersContext;

    public ARKResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers, IClickHouseService clickHouse, ILogger<ARKResolver> logger) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger)
    {
        _genericServersContext = serversContext;
    }


    public override string Name => "ARK";
    public override ulong AppId => 346110;


    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var arkServers = new List<ArkServer>(servers.Select(ResolveServerDetails));
        await Submit(arkServers);
    }

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.ArkServers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking().OrderBy(server => server.NextCheck).Take(50000)
            .ToListAsync();
        return servers.ToList<Server>();
    }

    private ArkServer ResolveServerDetails(IGenericServerInfo server)
    {
        var arkServer = new ArkServer();

        if (server.ExistingServer != null)
            arkServer.Copy(server.ExistingServer);
        // Update Game Name -- This may throw away some custom data returned by Game A2S_Info, but easier for queries to tell what game it is.
        arkServer.Game = Name;
        if (server.ServerRules != null) arkServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return arkServer;
    }
}