namespace UncoreMetrics.Data.GameData.Unturned;

public class UnturnedServer : Server
{
    [ServerRulesProperty("Browser_Desc_Full_Line_{0}")]
    public string? BrowserDescription { get; set; }


    [ServerRulesProperty("Browser_Desc_Hint")]
    public string? BrowserDescriptionHint { get; set; }

    [ServerRulesProperty("Browser_Icon")]
    public string? BrowserIcon { get; set; }


    [ServerRulesProperty("Custom_Link_Message_{0}", ValueType.Running)]
    public List<string>? CustomLinks { get; set; }


    [ServerRulesProperty("GameVersion")] public string? GameVersion { get; set; }


    [ServerRulesProperty("Mod_{0}", ValueType.Running)]
    public string? Mods { get; set; }

    [ServerRulesProperty("rocketplugins")]
    public string? RocketPlugins { get; set; }
}