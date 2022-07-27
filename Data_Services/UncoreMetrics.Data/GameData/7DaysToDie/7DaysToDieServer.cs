namespace UncoreMetrics.Data.GameData._7DaysToDie;

public class SevenDaysToDie : Server
{
    public int DayCount { get; set; }
    public int DayLightLength { get; set; }
    public int DayNightLength { get; set; }


    public int GameDifficulty { get; set; }
    public int LandClaimCount { get; set; }


    public int LandClaimDecayMode { get; set; }
    public int LandClaimExpiry { get; set; }

    public bool EAC { get; set; }
    public bool DropOnDeath { get; set; }
    public bool DropOnQuit { get; set; }
    public bool ShowFriendPlayerOnMap { get; set; }


    public bool IsPasswordProtected { get; set; }
    public bool IsPublic { get; set; }
    public bool RequiresMod { get; set; }

    public string Language { get; set; }

    public string ServerDescription { get; set; }

    public string ServerLoginConfirmationText { get; set; }

    public string ServerVersion { get; set; }
}