using Microsoft.Extensions.Options;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using Steam_Collector.SteamServers;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.ARK;

namespace Steam_Collector.Game_Collectors;

public class _7DaysToDie : BaseResolver
{

    public _7DaysToDie(
        IOptions<BaseConfiguration> baseConfiguration, ServersContext serversContext, ISteamServers steamServers) :
        base(baseConfiguration, serversContext, steamServers)
    {
    }






    public override string Name => "7DTD";
    public override ulong AppId => 251570;





    public override async Task HandleServersGeneric(List<IGenericServerInfo> servers)
    {

        List<ArkServer> arkServers = new List<ArkServer>(servers.Select(ResolveServerDetails));
        await Submit(arkServers);
    }

    private ArkServer ResolveServerDetails(IGenericServerInfo server)
    {
        ArkServer arkServer = new ArkServer();

        if (server.ExistingServer != null)
            arkServer.Copy(server.ExistingServer);

        if (server.ServerRules != null)
        {
            if (server.ServerRules.TryGetBooleanExtended("ALLOWDOWNLOADCHARS_i", out var allowDownloadChars))
                arkServer.DownloadCharacters = allowDownloadChars;


            if (server.ServerRules.TryGetBooleanExtended("ALLOWDOWNLOADITEMS_i", out var allowDownloadItems))
                arkServer.DownloadItems = allowDownloadItems;


            if (server.ServerRules.TryGetBooleanExtended("OFFICIALSERVER_i", out var officialServer))
                arkServer.OfficialServer = officialServer;

            if (server.ServerRules.TryGetBooleanExtended("SESSIONISPVE_i", out var PVEServer))
                arkServer.PVE = PVEServer;


            if (server.ServerRules.TryGetBooleanExtended("SERVERUSESBATTLEYE_b", out var hasBattleye))
                arkServer.Battleye = hasBattleye;


            if (server.ServerRules.TryGetBooleanExtended("ServerPassword_b", out var hasPassword))
                arkServer.PasswordRequired = hasPassword;

            if (server.ServerRules.TryGetBooleanExtended("HASACTIVEMODS_i", out var hasMods))
                arkServer.Modded = hasMods;


            if (server.ServerRules.TryGetInt("DayTime_s", out var daysRunning))
                arkServer.DaysRunning = daysRunning;


            if (server.ServerRules.TryGetInt("SESSIONFLAGS", out var sessionFlags))
                arkServer.SessionFlags = sessionFlags;


            if (server.ServerRules.TryGetString("ClusterId_s", out var clusterId))
                arkServer.ClusterID = clusterId;


            if (server.ServerRules.TryGetString("CUSTOMSERVERNAME_s", out var customServerName))
                arkServer.CustomServerName = customServerName;


            arkServer.Mods = server.ServerRules.TryGetRunningList("MOD{0}_s");
        }

        return arkServer;
    }
}
