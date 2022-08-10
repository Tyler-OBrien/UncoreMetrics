namespace UncoreMetrics.Data.GameData.ARK;

public class ArkServer : Server
{
    [ServerRulesProperty("HASACTIVEMODS_i")]
    public bool? Modded { get; set; }

    [ServerRulesProperty("ALLOWDOWNLOADCHARS_i")]
    public bool? DownloadCharacters { get; set; }

    [ServerRulesProperty("ALLOWDOWNLOADITEMS_i")]
    public bool? DownloadItems { get; set; }

    [ServerRulesProperty("MOD{0}_s", ValueType.Running)]
    public List<string>? Mods { get; set; }

    [ServerRulesProperty("DayTime_s")] public int? DaysRunning { get; set; }

    [ServerRulesProperty("SESSIONFLAGS")] public int? SessionFlags { get; set; }

    [ServerRulesProperty("ClusterId_s")] public string? ClusterID { get; set; }

    [ServerRulesProperty("CUSTOMSERVERNAME_s")]
    public string? CustomServerName { get; set; }

    [ServerRulesProperty("ServerPassword_b")]

    public bool? PasswordRequired { get; set; }

    [ServerRulesProperty("SERVERUSESBATTLEYE_b")]
    public bool? Battleye { get; set; }

    [ServerRulesProperty("OFFICIALSERVER_i")]


    public bool? OfficialServer { get; set; }

    [ServerRulesProperty("SESSIONISPVE_i")]

    public bool? PVE { get; set; }
}