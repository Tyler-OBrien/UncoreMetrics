namespace UncoreMetrics.Data.GameData.DayZ;

public class DayZServer : Server
{
    [GameDataRulesProperty("allowedBuild")]
    public bool? AllowedBuild { get; set; }

    [GameDataRulesProperty("island")] public string? Island { get; set; }

    [GameDataRulesProperty("language")] public string? Language { get; set; }

    [GameDataRulesProperty("requiredBuild")]
    public int? RequiredBuild { get; set; }

    [GameDataRulesProperty("requiredVersion")]
    public int? RequiredVersion { get; set; }

    [GameDataRulesProperty("timeLeft")] public int? TimeLeft { get; set; }
}