using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.Unturned;

namespace Steam_Collector.Game_Collectors;

public class UnturnedResolver : BaseResolver
{
    public UnturnedResolver(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers) :
        base(baseConfiguration, serversContext, steamServers)
    {
    }


    public override string Name => "Unturned";
    public override ulong AppId => 304930;


    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var customServers = new List<UnturnedServer>(servers.Select(ResolveServerDetails));
        await Submit(customServers);
    }

    private UnturnedServer ResolveServerDetails(IGenericServerInfo server)
    {
        UnturnedServer customServer = new();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);

        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return customServer;
    }
}