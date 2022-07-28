using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.Rust;

namespace Steam_Collector.Game_Collectors;

public class RustResolver : BaseResolver
{
    private readonly ServersContext _genericServersContext;

    public RustResolver(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers) :
        base(baseConfiguration, serversContext, steamServers)
    {
        _genericServersContext = serversContext;
    }


    public override string Name => "Rust";
    public override ulong AppId => 252490;

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.RustServers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking().OrderBy(server => server.NextCheck).Take(50000)
            .ToListAsync();
        return servers.ToList<Server>();
    }

    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var customServers = new List<RustServer>(servers.Select(ResolveServerDetails));
        await Submit(customServers);
    }

    private RustServer ResolveServerDetails(IGenericServerInfo server)
    {
        RustServer customServer = new();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);

        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return customServer;
    }
}