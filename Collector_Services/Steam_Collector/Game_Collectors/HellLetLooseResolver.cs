using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.HellLetLoose;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers;

namespace UncoreMetrics.Steam_Collector.Game_Collectors;

public class HellLetLooseResolver : BaseResolver
{
    private readonly ServersContext _genericServersContext;

    public HellLetLooseResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext,
        ISteamServers steamServers, IClickHouseService clickHouse, ILogger<HellLetLooseResolver> logger) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger)
    {
        _genericServersContext = serversContext;
    }


    public override string Name => "Hell Let Loose";
    public override ulong AppId => 686810;


    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.HellLetLooseServers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking()
            .OrderBy(server => server.NextCheck).Take(50000)
            .ToListAsync();
        // Abort run if less then 100 servers to poll, and no server is over 5 minutes overdue
        if (servers.Count < 100 && servers.Any(server => server.NextCheck > DateTime.UtcNow.AddMinutes(5)) == false)
            return new List<Server>();
        return servers.ToList<Server>();
    }

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
        // Update Game Name -- This may throw away some custom data returned by Game A2S_Info, but easier for queries to tell what game it is.
        customServer.Game = Name;

        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return customServer;
    }
}