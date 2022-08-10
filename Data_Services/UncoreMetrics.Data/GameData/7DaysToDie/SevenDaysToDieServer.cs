namespace UncoreMetrics.Data.GameData._7DaysToDie;

public class SevenDaysToDieServer : Server
{
    [ServerRulesProperty("AirDropMarker")] public bool? AirDropMarker { get; set; }


    [ServerRulesProperty("BedrollExpiryTime")]
    public int? BedrollExpiryTime { get; set; }

    [ServerRulesProperty("BloodMoonFrequency")]
    public string? BloodMoonFrequency { get; set; }


    [ServerRulesProperty("BuildCreate")] public bool? BuildCreate { get; set; }


    [ServerRulesProperty("CompatibilityVersion")]
    public string? CompatibilityVersion { get; set; }


    [ServerRulesProperty("CurrentServerTime")]
    public int? CurrentServerTime { get; set; }


    [ServerRulesProperty("DayCount")] public int? DayCount { get; set; }


    [ServerRulesProperty("DropOnDeath")] public bool? DropOnDeath { get; set; }


    [ServerRulesProperty("DropOnQuit")] public bool? DropOnQuit { get; set; }


    [ServerRulesProperty("EACEnabled")] public bool? EACEnabled { get; set; }


    [ServerRulesProperty("EnemyDifficulty")]
    public int? EnemyDifficulty { get; set; }


    [ServerRulesProperty("GameDifficulty")]
    public int? GameDifficulty { get; set; }


    [ServerRulesProperty("GameHost")] public string? GameHost { get; set; }


    [ServerRulesProperty("GameName")] public string? GameName { get; set; }


    [ServerRulesProperty("IsPasswordProtected")]
    public bool? IsPasswordProtected { get; set; }


    [ServerRulesProperty("IsPublic")] public bool? IsPublic { get; set; }


    [ServerRulesProperty("LandClaimCount")]
    public int? LandClaimCount { get; set; }


    [ServerRulesProperty("LandClaimDecayMode")]
    public int? LandClaimDecayMode { get; set; }


    [ServerRulesProperty("LandClaimExpiryTime")]
    public int? LandClaimExpiryTime { get; set; }


    [ServerRulesProperty("Language")] public string? Language { get; set; }


    [ServerRulesProperty("LevelName")] public string? LevelName { get; set; }


    [ServerRulesProperty("LootAbundance")] public int? LootAbundance { get; set; }


    [ServerRulesProperty("LootRespawnDays")]
    public int? LootRespawnDays { get; set; }


    [ServerRulesProperty("MaxSpawnedAnimals")]
    public int? MaxSpawnedAnimals { get; set; }


    [ServerRulesProperty("MaxSpawnedZombies")]
    public int? MaxSpawnedZombies { get; set; }


    [ServerRulesProperty("ModdedConfig")] public bool? ModdedConfig { get; set; }


    [ServerRulesProperty("PlayerKillingMode")]
    public int? PlayerKillingMode { get; set; }

    [ServerRulesProperty("Region")] public string? Region { get; set; }


    [ServerRulesProperty("RequiresMod")] public bool? RequiresMod { get; set; }


    [ServerRulesProperty("ServerDescription")]
    public string? ServerDescription { get; set; }


    [ServerRulesProperty("ServerLoginConfirmationText")]
    public string? ServerLoginConfirmationText { get; set; }


    [ServerRulesProperty("ServerWebsiteURL")]
    public string? ServerWebsiteURL { get; set; }


    [ServerRulesProperty("ShowFriendPlayerOnMap")]
    public bool? ShowFriendPlayerOnMap { get; set; }


    [ServerRulesProperty("StockFiles")] public bool? StockFiles { get; set; }


    [ServerRulesProperty("StockSettings")] public bool? StockSettings { get; set; }


    [ServerRulesProperty("Version")] public string? Version { get; set; }

    [ServerRulesProperty("WorldSize")] public int? WorldSize { get; set; }


    [ServerRulesProperty("XPMultiplier")] public int? XPMultiplier { get; set; }


    [ServerRulesProperty("ZombieBMMove")] public int? ZombieBMMove { get; set; }


    [ServerRulesProperty("ZombieFeralMove")]
    public int? ZombieFeralMove { get; set; }


    [ServerRulesProperty("ZombieFeralSense")]
    public int? ZombieFeralSense { get; set; }


    [ServerRulesProperty("ZombieMove")] public int? ZombieMove { get; set; }


    [ServerRulesProperty("ZombieMoveNight")]
    public int? ZombieMoveNight { get; set; }


    [ServerRulesProperty("ZombiesRun")] public int? ZombiesRun { get; set; }
}