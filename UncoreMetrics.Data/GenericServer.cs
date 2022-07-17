using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using NpgsqlTypes;

namespace UncoreMetrics.Data;

public class GenericServer
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public Guid ServerID { get; set; }

    [Required] public string Name { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public NpgsqlTsVector SearchVector { get; set; }


    [Required] public string Game { get; set; }

    [Required] public long AppID { get; set; }


    [Required]
    [MinLength(4)]
    [MaxLength(16)]
    public byte[] IpAddressBytes { get; set; }


    [Column(TypeName = "inet")] [Required] public IPAddress Address { get; set; }

    [Required] public int Port { get; set; }

    [Required] public int QueryPort { get; set; }

    [Required] public int Players { get; set; }

    [Required] public int MaxPlayers { get; set; }

    public long? ASN { get; set; }

    public string? ISP { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Country { get; set; }

    public string? Continent { get; set; }

    public string? Timezone { get; set; }

    [Required] public bool IsOnline { get; set; }

    [Required] public bool ServerDead { get; set; }


    [Required] public DateTime LastCheck { get; set; }

    [Required] public DateTime NextCheck { get; set; }

    [Required] public int FailedChecks { get; set; }

    [Required]
    public DateTime FoundAt { get; set; }
}