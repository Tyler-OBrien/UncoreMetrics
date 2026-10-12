using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.Arma3;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Helpers.QueueHelper;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers;

namespace UncoreMetrics.Steam_Collector.Game_Collectors;

public class Arma3Resolver : BaseResolver
{

    public Arma3Resolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext,
        ISteamServers steamServers, IClickHouseService clickHouse, ILogger<Arma3Resolver> logger, IServerUpdateQueue serverUpdateQueue) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger, serverUpdateQueue)
    {
    }


    public override string Name => "Arma 3";
    public override ulong AppId => 107410;


    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var customServers = new List<Arma3Server>(servers.Select(ResolveServerDetails));
        await Submit(customServers);
    }

    public override async Task<List<Server>> GetServers()
    {

        var servers = await _genericServersContext.Arma3Servers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking()
            .OrderBy(server => server.NextCheck).Take(_configuration.ServersPerPollRun)
            .ToListAsync();
        // Batch polls: wait for 1000 due servers, unless one has already waited longer than MaxPollDelay
        if (!ShouldPoll(servers, 1000, DateTime.UtcNow))
            return new List<Server>();
        return servers.ToList<Server>();
    }

    private Arma3Server ResolveServerDetails(IGenericServerInfo server)
    {
        Arma3Server customServer = new();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);
        // Update Game Name -- This may throw away some custom data returned by Game A2S_Info, but easier for queries to tell what game it is.
        customServer.Game = Name;
        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return customServer;
    }
}