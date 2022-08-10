namespace UncoreMetrics.Data.GameData.HellLetLoose;

public class HellLetLooseServer : Server
{
    [ServerRulesProperty("SESSIONFLAGS")] public int? SessionFlags { get; set; }

    [ServerRulesProperty("VISIB_i")] public int? Visible { get; set; }
}