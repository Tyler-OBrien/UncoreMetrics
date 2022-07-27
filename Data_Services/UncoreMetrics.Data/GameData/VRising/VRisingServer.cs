namespace UncoreMetrics.Data.GameData.VRising;

public enum CastleHeartDamageMode
{
    None,
    CanBeDestroyedOnlyWhenDecaying,
    CanBeDestroyedByPlayers,
    CanBeSeizedOrDestroyedByPlayers,
    Unknown
}

public class VRisingServer : Server
{
    [GameDataRulesProperty("castle-heart-damage-mode")]
    public CastleHeartDamageMode? HeartDamage { get; set; }

    [GameDataRulesProperty("blood-bound-enabled")]
    public bool? BloodBoundEquipment { get; set; }

    [GameDataRulesProperty("days-runningv2")]
    public int? DaysRunning { get; set; }

    [GameDataRulesProperty("desc{0}", ValueType.Running)]
    public string? Description { get; set; }
}