namespace UncoreMetrics.Data.GameData.Unturned;

public class UnturnedServer : Server
{
    [GameDataRulesProperty("Browser_Desc_Full_Line_{0}")]
    public string? BrowserDescription { get; set; }


    [GameDataRulesProperty("Browser_Desc_Hint")]
    public string? BrowserDescriptionHint { get; set; }

    [GameDataRulesProperty("Browser_Icon")]
    public string? BrowserIcon { get; set; }


    [GameDataRulesProperty("Custom_Link_Message_{0}", ValueType.Running)]
    public List<string>? CustomLinks { get; set; }


    [GameDataRulesProperty("GameVersion")] public string? GameVersion { get; set; }


    [GameDataRulesProperty("Mod_{0}", ValueType.Running)]
    public string? Mods { get; set; }

    [GameDataRulesProperty("rocketplugins")]
    public string? RocketPlugins { get; set; }
}