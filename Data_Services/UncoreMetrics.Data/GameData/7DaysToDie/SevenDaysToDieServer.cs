namespace UncoreMetrics.Data.GameData._7DaysToDie;

public class SevenDaysToDieServer : Server
{
    [GameDataRulesProperty("AirDropMarker")]
    public bool? AirDropMarker { get; set; }


    [GameDataRulesProperty("BedrollExpiryTime")]
    public int? BedrollExpiryTime { get; set; }

    [GameDataRulesProperty("BloodMoonFrequency")]
    public string? BloodMoonFrequency { get; set; }


    [GameDataRulesProperty("BuildCreate")] public bool? BuildCreate { get; set; }


    [GameDataRulesProperty("CompatibilityVersion")]
    public string? CompatibilityVersion { get; set; }


    [GameDataRulesProperty("CurrentServerTime")]
    public int? CurrentServerTime { get; set; }


    [GameDataRulesProperty("DayCount")] public int? DayCount { get; set; }


    [GameDataRulesProperty("DropOnDeath")] public bool? DropOnDeath { get; set; }


    [GameDataRulesProperty("DropOnQuit")] public bool? DropOnQuit { get; set; }


    [GameDataRulesProperty("EACEnabled")] public bool? EACEnabled { get; set; }


    [GameDataRulesProperty("EnemyDifficulty")]
    public int? EnemyDifficulty { get; set; }


    [GameDataRulesProperty("GameDifficulty")]
    public int? GameDifficulty { get; set; }


    [GameDataRulesProperty("GameHost")] public string? GameHost { get; set; }


    [GameDataRulesProperty("GameName")] public string? GameName { get; set; }


    [GameDataRulesProperty("IsPasswordProtected")]
    public bool? IsPasswordProtected { get; set; }


    [GameDataRulesProperty("IsPublic")] public bool? IsPublic { get; set; }


    [GameDataRulesProperty("LandClaimCount")]
    public int? LandClaimCount { get; set; }


    [GameDataRulesProperty("LandClaimDecayMode")]
    public int? LandClaimDecayMode { get; set; }


    [GameDataRulesProperty("LandClaimExpiryTime")]
    public int? LandClaimExpiryTime { get; set; }


    [GameDataRulesProperty("Language")] public string? Language { get; set; }


    [GameDataRulesProperty("LevelName")] public string? LevelName { get; set; }


    [GameDataRulesProperty("LootAbundance")]
    public int? LootAbundance { get; set; }


    [GameDataRulesProperty("LootRespawnDays")]
    public int? LootRespawnDays { get; set; }


    [GameDataRulesProperty("MaxSpawnedAnimals")]
    public int? MaxSpawnedAnimals { get; set; }


    [GameDataRulesProperty("MaxSpawnedZombies")]
    public int? MaxSpawnedZombies { get; set; }


    [GameDataRulesProperty("ModdedConfig")]
    public bool? ModdedConfig { get; set; }


    [GameDataRulesProperty("PlayerKillingMode")]
    public int? PlayerKillingMode { get; set; }

    [GameDataRulesProperty("Region")] public string? Region { get; set; }


    [GameDataRulesProperty("RequiresMod")] public bool? RequiresMod { get; set; }


    [GameDataRulesProperty("ServerDescription")]
    public string? ServerDescription { get; set; }


    [GameDataRulesProperty("ServerLoginConfirmationText")]
    public string? ServerLoginConfirmationText { get; set; }


    [GameDataRulesProperty("ServerWebsiteURL")]
    public string? ServerWebsiteURL { get; set; }


    [GameDataRulesProperty("ShowFriendPlayerOnMap")]
    public bool? ShowFriendPlayerOnMap { get; set; }


    [GameDataRulesProperty("StockFiles")] public bool? StockFiles { get; set; }


    [GameDataRulesProperty("StockSettings")]
    public bool? StockSettings { get; set; }


    [GameDataRulesProperty("Version")] public string? Version { get; set; }

    [GameDataRulesProperty("WorldSize")] public int? WorldSize { get; set; }


    [GameDataRulesProperty("XPMultiplier")]
    public int? XPMultiplier { get; set; }


    [GameDataRulesProperty("ZombieBMMove")]
    public int? ZombieBMMove { get; set; }


    [GameDataRulesProperty("ZombieFeralMove")]
    public int? ZombieFeralMove { get; set; }


    [GameDataRulesProperty("ZombieFeralSense")]
    public int? ZombieFeralSense { get; set; }


    [GameDataRulesProperty("ZombieMove")] public int? ZombieMove { get; set; }


    [GameDataRulesProperty("ZombieMoveNight")]
    public int? ZombieMoveNight { get; set; }


    [GameDataRulesProperty("ZombiesRun")] public int? ZombiesRun { get; set; }
}