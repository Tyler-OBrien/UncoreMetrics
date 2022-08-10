namespace UncoreMetrics.Data.GameData.PostScriptum;

public class PostScriptumServer : Server
{
    [ServerRulesProperty("AllModsWhitelisted_b")]
    public bool? AllModsWhitelisted { get; set; }

    [ServerRulesProperty("CurrentModLoadedCount_i")]
    public int? CurrentModLoadedCount { get; set; }


    [ServerRulesProperty("Flags_i")] public int? Flags { get; set; }


    [ServerRulesProperty("GameMode_s")] public string? GameMode { get; set; }

    [ServerRulesProperty("GameVersion_s")] public string? GameVersion { get; set; }


    [ServerRulesProperty("MatchTimeout_f")]
    public int? MatchTimeout { get; set; }


    [ServerRulesProperty("Password_b")] public bool? Password { get; set; }


    [ServerRulesProperty("PlayerReserveCount_i")]
    public int? PlayerReserveCount { get; set; }


    [ServerRulesProperty("PublicQueue_i")] public int? PublicQueue { get; set; }

    [ServerRulesProperty("PlayerCount_i")] public int? PlayerCount { get; set; }


    [ServerRulesProperty("ReservedQueue_i")]
    public int? ReservedQueue { get; set; }


    [ServerRulesProperty("SEARCHKEYWORDS_s")]
    public string? SearchKeywords { get; set; }


    [ServerRulesProperty("SESSIONFLAGS")] public int? SessionFlags { get; set; }
}