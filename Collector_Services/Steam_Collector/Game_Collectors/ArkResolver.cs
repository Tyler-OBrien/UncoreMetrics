using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.ARK;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Helpers.QueueHelper;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers;

namespace UncoreMetrics.Steam_Collector.Game_Collectors;

public class ARKResolver : BaseResolver
{

    public ARKResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext,
        ISteamServers steamServers, IClickHouseService clickHouse, ILogger<ARKResolver> logger, IServerUpdateQueue serverUpdateQueue) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger, serverUpdateQueue)
    {
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
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking()
            .OrderBy(server => server.NextCheck).Take(_configuration.ServersPerPollRun)
            .ToListAsync();
        // Abort run if less then 1000 servers to poll, and no server is over 5 minutes overdue
        if (servers.Count < 1000 && servers.Any(server => server.NextCheck > DateTime.UtcNow.AddMinutes(5)) == false)
            return new List<Server>();
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