namespace UncoreMetrics.Data.GameData.ProjectZomboid;

public class ProjectZomboidServer : Server
{
    [ServerRulesProperty("description")]
    [ServerRulesProperty("description:{0}/{1}", ValueType.Running, 1)]
    public string? Description { get; set; }

    [ServerRulesProperty("modCount")] public int? ModCount { get; set; }

    [ServerRulesProperty("mods")] public string? Mods { get; set; }


    [ServerRulesProperty("open")] public bool? Open { get; set; }


    [ServerRulesProperty("public")] public bool? Public { get; set; }


    [ServerRulesProperty("pvp")] public bool? PvP { get; set; }


    [ServerRulesProperty("version")] public string? Version { get; set; }
}