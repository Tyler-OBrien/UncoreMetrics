using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.VRising;

namespace Steam_Collector.Game_Collectors;

public class VRisingResolver : BaseResolver
{




    public VRisingResolver(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers) : base(baseConfiguration, serversContext, steamServers)
    {
        
    }



    public override string Name => "VRising";
    public override ulong AppId => 1604030;

    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {

        List<VRisingServer> vRisingServers = new List<VRisingServer>(servers.Select(ResolveServerDetails));
        await Submit(vRisingServers);
        
    }

    private VRisingServer ResolveServerDetails(IGenericServerInfo server)
    {
        var vRisingServer = new VRisingServer();
        if (server.ExistingServer != null)
            vRisingServer.Copy(server.ExistingServer);


        if (server.ServerRules != null)
        {
            vRisingServer.ResolveGameDataPropertiesFromRules(server.ServerRules);
            // Any Extra parsing
        }

        return vRisingServer;
    }
}
