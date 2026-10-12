using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncoreMetrics.Data.GameData
{
    public static class StaticGameInfo
    {
        public static readonly ReadOnlyCollection<GameInformation> Games = new ReadOnlyCollection<GameInformation>(
            new List<GameInformation>()
            {
                new GameInformation(346110, "ARK", "ARK"),
                new GameInformation(107410, "Arma 3", "Arma3"),
                new GameInformation(221100, "DayZ", "DayZ"),
                new GameInformation(686810, "Hell Let Loose", "Hell-Let-Loose"),
                new GameInformation(736220, "Post Scriptum", "Post-Scriptum"),
                new GameInformation(108600, "Project Zomboid", "Project-Zomboid"),
                new GameInformation(252490, "Rust", "Rust"),
                new GameInformation(251570, "7 Days To Die", "7-days-to-die"),
                new GameInformation(393380, "Squad", "Squad"),
                new GameInformation(304930, "Unturned", "Unturned"),
                new GameInformation(1604030, "V Rising", "V-Rising"),
                // Tracked by the collector's GenericResolver (no game-specific table)
                new GameInformation(730, "Counter-Strike 2", "Counter-Strike-2"),
                new GameInformation(892970, "Valheim", "Valheim"),
                new GameInformation(550, "Left 4 Dead 2", "Left-4-Dead-2"),
                new GameInformation(4000, "Garry's Mod", "Garrys-Mod"),
                new GameInformation(440, "Team Fortress 2", "Team-Fortress-2"),
                new GameInformation(1203620, "Enshrouded", "Enshrouded"),
                new GameInformation(1149460, "Icarus", "Icarus"),
                new GameInformation(244850, "Space Engineers", "Space-Engineers"),
            });
    }

    public class GameInformation
    {
        public GameInformation(ulong appid, string name, string slugName)
        {
            AppId = appid; 
            Name = name;
            SlugName = slugName;
        }

        public ulong AppId { get; }

        public string Name { get;}

        public string SlugName { get;}
    }
}
