using UncoreMetrics.Steam_Collector.Game_Collectors;

namespace UncoreMetrics.Steam_Collector;

public class GameResolvers
{
    private static readonly Dictionary<string, Type> _gameResolvers =
        new(StringComparer.OrdinalIgnoreCase)
        {
            {
                "VRising",
                typeof(VRisingResolver)
            },
            {
                "ARK",
                typeof(ARKResolver)
            },
            {
                "ProjectZomboid",
                typeof(ProjectZomboidResolver)
            },
            {
                "7DTD",
                typeof(SevenDaysToDieResolver)
            },
            {
                "Arma3",
                typeof(Arma3Resolver)
            },
            {
                "DayZ",
                typeof(DayZResolver)
            },
            {
                "HellLetLoose",
                typeof(HellLetLooseResolver)
            },
            {
                "PostScriptum",
                typeof(PostScriptumResolver)
            },
            {
                "Rust",
                typeof(RustResolver)
            },
            {
                "Unturned",
                typeof(UnturnedResolver)
            }
            ,
            {
                "Squad",
                typeof(SquadResolver)
            }
        };

    public bool DoesGameResolverExist(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return _gameResolvers.ContainsKey(name) || GenericGames.All.ContainsKey(name);
    }

    /// <summary>The generic game for this type, or null when it has a dedicated resolver.</summary>
    public GenericGame? GetGenericGame(string name)
    {
        return !_gameResolvers.ContainsKey(name) && GenericGames.All.TryGetValue(name, out var game) ? game : null;
    }

    public Type GetResolver(string name)
    {
        return _gameResolvers[name];
    }

    public string GetValidResolvers()
    {
        return string.Join(", ", _gameResolvers.Keys.Concat(GenericGames.All.Keys));
    }
}