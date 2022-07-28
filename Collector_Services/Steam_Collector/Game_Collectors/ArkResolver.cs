using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.ARK;

namespace Steam_Collector.Game_Collectors;

public class ARKResolver : BaseResolver
{
    public ARKResolver(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers) :
        base(baseConfiguration, serversContext, steamServers)
    {
    }


    public override string Name => "ARK";
    public override ulong AppId => 346110;


    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var arkServers = new List<ArkServer>(servers.Select(ResolveServerDetails));
        await Submit(arkServers);
    }

    private ArkServer ResolveServerDetails(IGenericServerInfo server)
    {
        var arkServer = new ArkServer();

        if (server.ExistingServer != null)
            arkServer.Copy(server.ExistingServer);

        if (server.ServerRules != null) arkServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return arkServer;
    }
}