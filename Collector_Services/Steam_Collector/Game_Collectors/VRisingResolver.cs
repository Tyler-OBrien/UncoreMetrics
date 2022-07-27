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



    public override string Name => "RVising";
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
            if (server.ServerRules.TryGetBoolean("blood-bound-enabled", out var bloodBound))
                vRisingServer.BloodBoundEquipment = bloodBound;
            if (server.ServerRules.TryGetEnum("castle-heart-damage-mode",
                    out CastleHeartDamageMode castleHeartDamageMode))
                vRisingServer.HeartDamage = castleHeartDamageMode;
            if (server.ServerRules.TryGetInt("days-runningv2", out var daysRunning))
                vRisingServer.DaysRunning = daysRunning;
            if (server.ServerRules.TryGetRunningString("desc{0}", out var description))
                vRisingServer.Description = description;
        }

        return vRisingServer;
    }
}
