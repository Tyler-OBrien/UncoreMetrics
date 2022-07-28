using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.ProjectZomboid;

namespace Steam_Collector.Game_Collectors;

public class ProjectZomboidResolver : BaseResolver
{
    public ProjectZomboidResolver(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers) :
        base(baseConfiguration, serversContext, steamServers)
    {
    }


    public override string Name => "ProjectZomboid";
    public override ulong AppId => 108600;


    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var projectZomboidServers = new List<ProjectZomboidServer>(servers.Select(ResolveServerDetails));
        await Submit(projectZomboidServers);
    }

    private ProjectZomboidServer ResolveServerDetails(IGenericServerInfo server)
    {
        ProjectZomboidServer customServer = new();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);


        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return customServer;
    }
}