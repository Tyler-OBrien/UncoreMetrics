using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncoreMetrics.Data.GameData.VRising
{
    public enum CastleHeartDamageMode
    {
        None,
        CanBeDestroyedOnlyWhenDecaying,
        CanBeDestroyedByPlayers,
        CanBeSeizedOrDestroyedByPlayers,
        Unknown
    }
    public class VRisingServer : GenericServer
    {
        public VRisingServer()
        {

        }
        public VRisingServer(CastleHeartDamageMode? heartDamage, bool? bloodBoundEquipment, int? daysRunning, string? description)
        {
            HeartDamage = heartDamage;
            BloodBoundEquipment = bloodBoundEquipment;
            DaysRunning = daysRunning;
            Description = description;
        }
     



        public CastleHeartDamageMode? HeartDamage { get; set; }

        public bool? BloodBoundEquipment { get; set; }

        public int? DaysRunning { get; set; }

        public string? Description { get; set; }
    }
}
