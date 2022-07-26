using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steam_Collector.Helpers;
using Steam_Collector.Models;
using Steam_Collector.Models.Games.Steam.SteamAPI;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData.VRising;

namespace Steam_Collector.Game_Collectors
{
    public class VRisingResolver : IGameResolver
    {
        // We might be able to implement this by just using attributes in the future
        public string Name => "RVising";
        public Type ServerType => typeof(VRisingServer);

        public void ResolveCustomServerInfo<T>(IGenericServerInfo<T> server) where T : Server, new()
        {
            if (server.CustomServerInfo is not VRisingServer)
            {
                throw new InvalidOperationException("This Resolver only works for VRisingServers");
            }

            ResolveCustomServerInfoVRising((IGenericServerInfo<VRisingServer>)server);
        }

        public void ResolveCustomServerInfoVRising(IGenericServerInfo<VRisingServer> server)
        {

                if (server.ServerRules != null)
                {

                    if (server.ServerRules.TryGetBoolean("blood-bound-enabled", out var bloodBound))
                        server.CustomServerInfo.BloodBoundEquipment = bloodBound;
                    if (server.ServerRules.TryGetEnum("castle-heart-damage-mode", out CastleHeartDamageMode castleHeartDamageMode))
                        server.CustomServerInfo.HeartDamage = castleHeartDamageMode;
                    if (server.ServerRules.TryGetInt("days-runningv2", out var daysRunning))
                        server.CustomServerInfo.DaysRunning = daysRunning;
                    if (server.ServerRules.TryGetRunningString("desc{0}", out var description))
                        server.CustomServerInfo.Description = description;
                }
        }

    }
}
