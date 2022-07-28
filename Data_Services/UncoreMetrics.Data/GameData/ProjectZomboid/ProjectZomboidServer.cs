namespace UncoreMetrics.Data.GameData.ProjectZomboid;

public class ProjectZomboidServer : Server
{
    [GameDataRulesProperty("description")]
    [GameDataRulesProperty("description:{0}/{1}", ValueType.Running, 1)]
    public string? Description { get; set; }

    [GameDataRulesProperty("modCount")] public int? ModCount { get; set; }

    [GameDataRulesProperty("mods")] public string? Mods { get; set; }


    [GameDataRulesProperty("open")] public bool? Open { get; set; }


    [GameDataRulesProperty("public")] public bool? Public { get; set; }


    [GameDataRulesProperty("pvp")] public bool? PvP { get; set; }


    [GameDataRulesProperty("version")] public string? Version { get; set; }
}