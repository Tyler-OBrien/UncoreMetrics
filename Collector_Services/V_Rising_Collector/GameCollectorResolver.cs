using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steam_Collector.Game_Collectors;
using Steam_Collector.Models;

namespace Steam_Collector
{
    public static class GameCollectorResolver
    {
        private static readonly Dictionary<string, Lazy<IGameResolver>> _gameResolvers =
            new (StringComparer.OrdinalIgnoreCase)
            {
                {
                    "VRising",
                    new Lazy<IGameResolver>((() => new VRisingResolver()))
                },
                {
                    "ARK",
                    new Lazy<IGameResolver>((() => new ArkResolver()))
                }
            };
        public static bool DoesGameResolverExist(string name)
        {
            return _gameResolvers.ContainsKey(name);
        }

        public static IGameResolver GetResolver(string name)
        {
            return _gameResolvers[name].Value;
        }

        public static string GetValidResolvers()
        {
            return String.Join(", ", _gameResolvers.Keys.ToList());
        }

    }
}
