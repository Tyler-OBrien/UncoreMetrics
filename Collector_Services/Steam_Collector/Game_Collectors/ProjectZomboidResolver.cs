﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.ProjectZomboid;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Helpers.QueueHelper;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers;

namespace UncoreMetrics.Steam_Collector.Game_Collectors;

public class ProjectZomboidResolver : BaseResolver
{

    public ProjectZomboidResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext,
        ISteamServers steamServers, IClickHouseService clickHouse, ILogger<ProjectZomboidResolver> logger, IServerUpdateQueue serverUpdateQueue) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger, serverUpdateQueue)
    {
    }


    public override string Name => "Project Zomboid";
    public override ulong AppId => 108600;

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.ProjectZomboidServers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking()
            .OrderBy(server => server.NextCheck).Take(_configuration.ServersPerPollRun)
            .ToListAsync();
        // Abort run if less then 10 servers to poll, and no server is over 5 minutes overdue
        if (servers.Count < 10 && servers.Any(server => server.NextCheck > DateTime.UtcNow.AddMinutes(5)) == false)
            return new List<Server>();
        return servers.ToList<Server>();
    }

    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var projectZomboidServers = new List<ProjectZomboidServer>(servers.Select(ResolveServerDetails));
        await Submit(projectZomboidServers);
    }

    private ProjectZomboidServer ResolveServerDetails(IGenericServerInfo server)
    {
        ProjectZomboidServer customServer = new();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);
        // Update Game Name -- This may throw away some custom data returned by Game A2S_Info, but easier for queries to tell what game it is.
        customServer.Game = Name;

        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return customServer;
    }
}