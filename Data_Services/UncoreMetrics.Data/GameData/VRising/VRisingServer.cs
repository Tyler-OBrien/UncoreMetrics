using System.ComponentModel.DataAnnotations;

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
    public CastleHeartDamageMode? HeartDamage { get; set; }

    public bool? BloodBoundEquipment { get; set; }

    public int? DaysRunning { get; set; }

    public string? Description { get; set; }
}