namespace UncoreMetrics.Data.GameData.HellLetLoose;

public class HellLetLooseServer : Server
{
    [GameDataRulesProperty("SESSIONFLAGS")]

    public int? SessionFlags { get; set; }

    [GameDataRulesProperty("VISIB_i")] public int? Visible { get; set; }
}