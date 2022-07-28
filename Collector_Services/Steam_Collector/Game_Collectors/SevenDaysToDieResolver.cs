using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData._7DaysToDie;

namespace Steam_Collector.Game_Collectors;

public class SevenDaysToDieResolver : BaseResolver
{
    public SevenDaysToDieResolver(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers) :
        base(baseConfiguration, serversContext, steamServers)
    {
    }


    public override string Name => "7DTD";
    public override ulong AppId => 251570;


    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {
        var sevenDTDServers = new List<SevenDaysToDieServer>(servers.Select(ResolveServerDetails));
        await Submit(sevenDTDServers);
    }

    private SevenDaysToDieServer ResolveServerDetails(IGenericServerInfo server)
    {
        var customServer = new SevenDaysToDieServer();

        if (server.ExistingServer != null)
            customServer.Copy(server.ExistingServer);

        if (server.ServerRules != null) customServer.ResolveGameDataPropertiesFromRules(server.ServerRules);

        return customServer;
    }
}