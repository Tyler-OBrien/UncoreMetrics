namespace UncoreMetrics.Data.GameData.ARK;

public class ArkServer : Server
{
    [GameDataRulesProperty("HASACTIVEMODS_i")]
    public bool? Modded { get; set; }

    [GameDataRulesProperty("ALLOWDOWNLOADCHARS_i")]
    public bool? DownloadCharacters { get; set; }

    [GameDataRulesProperty("ALLOWDOWNLOADITEMS_i")]
    public bool? DownloadItems { get; set; }

    [GameDataRulesProperty("MOD{0}_s", ValueType.Running)]
    public List<string>? Mods { get; set; }

    [GameDataRulesProperty("DayTime_s")] public int? DaysRunning { get; set; }

    [GameDataRulesProperty("SESSIONFLAGS")]
    public int? SessionFlags { get; set; }

    [GameDataRulesProperty("ClusterId_s")] public string? ClusterID { get; set; }

    [GameDataRulesProperty("CUSTOMSERVERNAME_s")]
    public string? CustomServerName { get; set; }

    [GameDataRulesProperty("ServerPassword_b")]

    public bool? PasswordRequired { get; set; }

    [GameDataRulesProperty("SERVERUSESBATTLEYE_b")]
    public bool? Battleye { get; set; }

    [GameDataRulesProperty("OFFICIALSERVER_i")]


    public bool? OfficialServer { get; set; }

    [GameDataRulesProperty("SESSIONISPVE_i")]

    public bool? PVE { get; set; }
}