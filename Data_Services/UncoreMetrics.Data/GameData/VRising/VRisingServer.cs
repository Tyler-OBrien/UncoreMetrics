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
    [ServerRulesProperty("castle-heart-damage-mode")]
    public CastleHeartDamageMode? HeartDamage { get; set; }

    [ServerRulesProperty("blood-bound-enabled")]
    public bool? BloodBoundEquipment { get; set; }

    [ServerRulesProperty("days-runningv2")]
    public int? DaysRunning { get; set; }

    [ServerRulesProperty("desc{0}", ValueType.Running)]
    public string? Description { get; set; }
}