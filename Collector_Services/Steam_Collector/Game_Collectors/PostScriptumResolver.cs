using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.GameData.PostScriptum;
using UncoreMetrics.Steam_Collector.Helpers;
using UncoreMetrics.Steam_Collector.Helpers.QueueHelper;
using UncoreMetrics.Steam_Collector.Models;
using UncoreMetrics.Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Steam_Collector.SteamServers;
using UncoreMetrics.Steam_Collector.SteamServers.WebAPI;

namespace UncoreMetrics.Steam_Collector.Game_Collectors;

public class PostScriptumResolver : BaseResolver
{
    private readonly ServersContext _genericServersContext;

    public PostScriptumResolver(
        IOptions<SteamCollectorConfiguration> baseConfiguration, ServersContext serversContext,
        ISteamServers steamServers, IClickHouseService clickHouse, ILogger<PostScriptumResolver> logger, IServerUpdateQueue serverUpdateQueue) :
        base(baseConfiguration, serversContext, steamServers, clickHouse, logger, serverUpdateQueue)
    {
        _genericServersContext = serversContext;
    }


    public override string Name => "Post Scriptum";
    public override ulong AppId => 736220;

    // Post Scriptum servers don't report their player count to steam, only visible through A2S_Rules...
    public override SteamServerListQueryBuilder? CustomQuery =>
        SteamServerListQueryBuilder.New().AppID(AppId.ToString()).Dedicated();

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.PostScriptumServers
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
        var customServers = new List<PostScriptumServer>(servers.Select(ResolveServerDetails));
        await Submit(customServers);
    }

    private PostScriptumServer ResolveServerDetails(IGenericServerInfo server)
    {
        PostScriptumServer customServer = new();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);

        // Update Game Name -- This may throw away some custom data returned by Game A2S_Info, but easier for queries to tell what game it is.
        customServer.Game = Name;

        if (server.ServerRules != null)
        {
            customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);
            customServer.Players = ((uint)customServer.PlayerCount)!;
        }

        return customServer;
    }
}