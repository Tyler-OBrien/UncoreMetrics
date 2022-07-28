namespace UncoreMetrics.Data.GameData.PostScriptum;

public class PostScriptumServer : Server
{
    [GameDataRulesProperty("AllModsWhitelisted_b")]
    public bool? AllModsWhitelisted { get; set; }

    [GameDataRulesProperty("CurrentModLoadedCount_i")]
    public int? CurrentModLoadedCount { get; set; }


    [GameDataRulesProperty("Flags_i")] public int? Flags { get; set; }


    [GameDataRulesProperty("GameMode_s")] public string? GameMode { get; set; }

    [GameDataRulesProperty("GameVersion_s")]
    public string? GameVersion { get; set; }


    [GameDataRulesProperty("MatchTimeout_f")]
    public int? MatchTimeout { get; set; }


    [GameDataRulesProperty("Password_b")] public bool? Password { get; set; }


    [GameDataRulesProperty("PlayerReserveCount_i")]
    public int? PlayerReserveCount { get; set; }


    [GameDataRulesProperty("PublicQueue_i")]
    public int? PublicQueue { get; set; }


    [GameDataRulesProperty("ReservedQueue_i")]
    public int? ReservedQueue { get; set; }


    [GameDataRulesProperty("SEARCHKEYWORDS_s")]
    public string? SearchKeywords { get; set; }


    [GameDataRulesProperty("SESSIONFLAGS")]
    public int? SessionFlags { get; set; }
}