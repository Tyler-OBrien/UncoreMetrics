namespace UncoreMetrics.Data.GameData.DayZ;

public class DayZServer : Server
{
    [ServerRulesProperty("allowedBuild")]
    public bool? AllowedBuild { get; set; }

    [ServerRulesProperty("island")] public string? Island { get; set; }

    [ServerRulesProperty("language")] public string? Language { get; set; }

    [ServerRulesProperty("requiredBuild")]
    public int? RequiredBuild { get; set; }

    [ServerRulesProperty("requiredVersion")]
    public int? RequiredVersion { get; set; }

    [ServerRulesProperty("timeLeft")] public int? TimeLeft { get; set; }
}