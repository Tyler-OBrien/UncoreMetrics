using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steam_Collector.Game_Collectors;

namespace Steam_Collector
{
    public class GameResolvers
    {
        private static readonly Dictionary<string, Type> _gameResolvers =
            new (StringComparer.OrdinalIgnoreCase)
            {
                {
                    "VRising",
                    typeof(VRisingResolver)
                },
                {
                    "ARK",
                    typeof(VRisingResolver)
                }
            };
        public bool DoesGameResolverExist(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return false;
            return _gameResolvers.ContainsKey(name);
        }

        public Type GetResolver(string name)
        {
            return _gameResolvers[name];
        }

        public string GetValidResolvers()
        {
            return String.Join(", ", _gameResolvers.Keys.ToList());
        }

    }
}
