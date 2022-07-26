using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.ARK;
using UncoreMetrics.Data.GameData.VRising;

namespace Steam_Collector.Game_Collectors
{
    public class ArkResolver : IGameResolver
    {
        // We might be able to implement this by just using attributes in the future
        public string Name => "ARK";

        public Type ServerType => typeof(ArkServer);

        public void ResolveCustomServerInfo<T>(IGenericServerInfo<T> server) where T : Server, new()
        {
            if (server.CustomServerInfo is not ArkServer)
            {
                throw new InvalidOperationException("This Resolver only works for VRisingServers");
            }

            ResolveCustomServerInfo((IGenericServerInfo<ArkServer>)server);
        }

        public void ResolveCustomServerInfo(IGenericServerInfo<ArkServer> server)
        {
            if (server.ServerRules != null)
                {

                    if (server.ServerRules.TryGetBooleanExtended("ALLOWDOWNLOADCHARS_i", out var allowDownloadChars))
                        server.CustomServerInfo.DownloadCharacters = allowDownloadChars;


                    if (server.ServerRules.TryGetBooleanExtended("ALLOWDOWNLOADITEMS_i", out var allowDownloadItems))
                        server.CustomServerInfo.DownloadItems = allowDownloadItems;


                    if (server.ServerRules.TryGetBooleanExtended("OFFICIALSERVER_i", out var officialServer))
                        server.CustomServerInfo.OfficialServer = officialServer;

                    if (server.ServerRules.TryGetBooleanExtended("SESSIONISPVE_i", out var PVEServer))
                        server.CustomServerInfo.PVE = PVEServer;


                    if (server.ServerRules.TryGetBooleanExtended("SERVERUSESBATTLEYE_b", out var hasBattleye))
                        server.CustomServerInfo.Battleye = hasBattleye;


                    if (server.ServerRules.TryGetBooleanExtended("ServerPassword_b", out var hasPassword))
                        server.CustomServerInfo.PasswordRequired = hasPassword;

                    if (server.ServerRules.TryGetBooleanExtended("HASACTIVEMODS_i", out var hasMods))
                        server.CustomServerInfo.Modded = hasMods;


                    if (server.ServerRules.TryGetInt("DayTime_s", out var daysRunning))
                        server.CustomServerInfo.DaysRunning = daysRunning;


                    if (server.ServerRules.TryGetInt("SESSIONFLAGS", out var sessionFlags))
                        server.CustomServerInfo.SessionFlags = sessionFlags;


                    if (server.ServerRules.TryGetString("ClusterId_s", out var clusterId))
                        server.CustomServerInfo.ClusterID = clusterId;



                    if (server.ServerRules.TryGetString("CUSTOMSERVERNAME_s", out var customServerName))
                        server.CustomServerInfo.CustomServerName = customServerName;


                    server.CustomServerInfo.Mods = server.ServerRules.TryGetRunningList("MOD{0}_s");
                }
        }

    }
}
