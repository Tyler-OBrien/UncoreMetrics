using Steam_Collector.Game_Collectors;

namespace Steam_Collector;

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
        };

    public bool DoesGameResolverExist(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return _gameResolvers.ContainsKey(name);
    }

    public Type GetResolver(string name)
    {
        return _gameResolvers[name];
    }

    public string GetValidResolvers()
    {
        return string.Join(", ", _gameResolvers.Keys.ToList());
    }
}