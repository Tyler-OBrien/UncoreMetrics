using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using NpgsqlTypes;

namespace UncoreMetrics.Data;

public class GenericServer
{


    public GenericServer()
    { }

    public void Copy(GenericServer toCopy)
    {
        this.ServerID = toCopy.ServerID;
        this.Name = toCopy.Name;
        this.SearchVector = toCopy.SearchVector;
        this.Game = toCopy.Game;
        this.AppID = toCopy.AppID;
        this.IpAddressBytes = toCopy.IpAddressBytes;
        this.Address = toCopy.Address;
        this.Port = toCopy.Port;
        this.QueryPort = toCopy.QueryPort;
        this.Players = toCopy.Players;
        this.MaxPlayers = toCopy.MaxPlayers;
        this.ASN = toCopy.ASN;
        this.ISP = toCopy.ISP;
        this.Latitude = toCopy.Latitude;
        this.Longitude = toCopy.Longitude;
        this.Country = toCopy.Country;
        this.Continent = toCopy.Continent;
        this.Timezone = toCopy.Timezone;
        this.IsOnline = toCopy.IsOnline;
        this.ServerDead = toCopy.ServerDead;
        this.LastCheck = toCopy.LastCheck;
        this.NextCheck = toCopy.NextCheck;
        this.FailedChecks = toCopy.FailedChecks;
        this.FoundAt = toCopy.FoundAt;
    }



    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public Guid ServerID { get; set; }

    [Required] public string Name { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public NpgsqlTsVector SearchVector { get; set; }


    [Required] public string Game { get; set; }

    [Required] public ulong AppID { get; set; }


    [Required]
    [MinLength(4)]
    [MaxLength(16)]
    public byte[] IpAddressBytes { get; set; }


    [Column(TypeName = "inet")] [Required] public IPAddress Address { get; set; }

    [Required] public int Port { get; set; }

    [Required] public int QueryPort { get; set; }

    [Required] public uint Players { get; set; }

    [Required] public uint MaxPlayers { get; set; }

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