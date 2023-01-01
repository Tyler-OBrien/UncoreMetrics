using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.Rust;
using UncoreMetrics.Data.GameData.Squad;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Helpers.QueueHelper;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers;

namespace UncoreMetrics.Steam_Collector.Game_Collectors;

public class SquadResolver : BaseResolver
{

    public SquadResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext,
        ISteamServers steamServers, IClickHouseService clickHouse, ILogger<RustResolver> logger, IServerUpdateQueue serverUpdateQueue) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger, serverUpdateQueue)
    {
    }


    public override string Name => "Squad";
    public override ulong AppId => 393380;

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.SquadServers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking()
            .OrderBy(server => server.NextCheck).Take(_configuration.ServersPerPollRun)
            .ToListAsync();
        // Abort run if less then 100 servers to poll, and no server is over 5 minutes overdue
        if (servers.Count < 100 && servers.Any(server => server.NextCheck > DateTime.UtcNow.AddMinutes(5)) == false)
            return new List<Server>();
        return servers.ToList<Server>();
    }

    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var customServers = new List<SquadServer>(servers.Select(ResolveServerDetails));
        await Submit(customServers);
    }

    private SquadServer ResolveServerDetails(IGenericServerInfo server)
    {
        SquadServer customServer = new();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);
        // Update Game Name -- This may throw away some custom data returned by Game A2S_Info, but easier for queries to tell what game it is.
        customServer.Game = Name;

        if (server.ServerPlayers != null) customServer.Players = (uint)server.ServerPlayers.Players.Count;
        if (server.ServerRules != null)
        {
            customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);
            if (customServer.PlayerCount != null)
                customServer.Players = ((uint)customServer.PlayerCount)!;

        }

        return customServer;
    }
}