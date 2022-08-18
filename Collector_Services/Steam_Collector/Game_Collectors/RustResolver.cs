using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.Rust;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers;

namespace UncoreMetrics.Steam_Collector.Game_Collectors;

public class RustResolver : BaseResolver
{
    private readonly ServersContext _genericServersContext;

    public RustResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext,
        ISteamServers steamServers, IClickHouseService clickHouse, ILogger<RustResolver> logger) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger)
    {
        _genericServersContext = serversContext;
    }


    public override string Name => "Rust";
    public override ulong AppId => 252490;

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.RustServers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking()
            .OrderBy(server => server.NextCheck).Take(50000)
            .ToListAsync();
        // Abort run if less then 1000 servers to poll, and no server is over 5 minutes overdue
        if (servers.Count < 1000 && servers.Any(server => server.NextCheck > DateTime.UtcNow.AddMinutes(5)) == false)
            return new List<Server>();
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
        // Update Game Name -- This may throw away some custom data returned by Game A2S_Info, but easier for queries to tell what game it is.
        customServer.Game = Name;

        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);
        if (server.ServerPlayers != null) customServer.Players = (uint)server.ServerPlayers.Players.Count;

        return customServer;
    }
}