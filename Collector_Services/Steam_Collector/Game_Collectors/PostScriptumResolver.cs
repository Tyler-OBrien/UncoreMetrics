using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.PostScriptum;

namespace Steam_Collector.Game_Collectors;

public class PostScriptumResolver : BaseResolver
{
    public PostScriptumResolver(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers) :
        base(baseConfiguration, serversContext, steamServers)
    {
    }


    public override string Name => "PostScriptum";
    public override ulong AppId => 736220;


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

        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return customServer;
    }
}