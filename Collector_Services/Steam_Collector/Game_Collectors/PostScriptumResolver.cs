using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using Steam_Collector.SteamServers.WebAPI;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.PostScriptum;

namespace Steam_Collector.Game_Collectors;

public class PostScriptumResolver : BaseResolver
{
    private readonly ServersContext _genericServersContext;

    public PostScriptumResolver(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers) :
        base(baseConfiguration, serversContext, steamServers)
    {
        _genericServersContext = serversContext;
    }


    public override string Name => "PostScriptum";
    public override ulong AppId => 736220;

    // Post Scriptum servers don't report their player count to steam, only visible through A2S_Rules...
    public override SteamServerListQueryBuilder? CustomQuery =>
        SteamServerListQueryBuilder.New().AppID(AppId.ToString()).Dedicated();

    public override async Task<List<Server>> GetServers()
    {
        var servers = await _genericServersContext.PostScriptumServers
            .Where(server => server.NextCheck < DateTime.UtcNow && server.AppID == AppId).AsNoTracking().OrderBy(server => server.NextCheck).Take(50000)
            .ToListAsync();
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

        if (server.ServerRules != null)
        {
            customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);
            customServer.Players = ((uint)customServer.PlayerCount)!;
        }

        return customServer;
    }
}