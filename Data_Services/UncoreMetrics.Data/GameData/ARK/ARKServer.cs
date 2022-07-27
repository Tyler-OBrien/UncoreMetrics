using System.ComponentModel.DataAnnotations;

namespace UncoreMetrics.Data.GameData.ARK;

public class ArkServer : Server
{

    public bool? Modded { get; set; }


    public bool? DownloadCharacters { get; set; }
    public bool? DownloadItems { get; set; }


    public List<string>? Mods { get; set; }

    public int? DaysRunning { get; set; }
    public int? SessionFlags { get; set; }

    public string? ClusterID { get; set; }

    public string? CustomServerName { get; set; }

    public bool? PasswordRequired { get; set; }

    public bool? Battleye { get; set; }

    public bool? OfficialServer { get; set; }

    public bool? PVE { get; set; }
}