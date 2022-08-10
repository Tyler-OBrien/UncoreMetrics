using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.VRising;

namespace Steam_Collector.Game_Collectors;

public class VRisingResolver : BaseResolver
{
    private readonly ServersContext _genericServersContext;

    public VRisingResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers, IClickHouseService clickHouse, ILogger<VRisingResolver> logger) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger)
    {
        _genericServersContext = serversContext;
    }


    public override string Name => "V Rising";
    public override ulong AppId => 1604030;

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.VRisingServers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking().OrderBy(server => server.NextCheck).Take(50000)
            .ToListAsync();
        return servers.ToList<Server>();
    }

    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var vRisingServers = new List<VRisingServer>(servers.Select(ResolveServerDetails));
        await Submit(vRisingServers);
    }

    private VRisingServer ResolveServerDetails(IGenericServerInfo server)
    {
        var vRisingServer = new VRisingServer();
        if (server.ExistingServer != null)
            vRisingServer.Copy(server.ExistingServer);
        // Update Game Name -- This may throw away some custom data returned by Game A2S_Info, but easier for queries to tell what game it is.
        vRisingServer.Game = Name;

        if (server.ServerRules != null)
            vRisingServer.ResolveGameDataPropertiesFromRules(server.ServerRules);
        // Any Extra parsing

        return vRisingServer;
    }
}