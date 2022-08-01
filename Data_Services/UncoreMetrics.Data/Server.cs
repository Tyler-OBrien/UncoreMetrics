using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using NpgsqlTypes;

namespace UncoreMetrics.Data;

public enum Continent
{
    Unknown = 1,
    AF,
    AN,
    AS,
    EU,
    NA,
    OC,
    SA,
}

public class Server
{
    [Required] public Guid ServerID { get; set; }

    [Required] public string Name { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public NpgsqlTsVector SearchVector { get; set; }


    [Required] public string Game { get; set; }

    public string Map { get; set; }

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

    public bool? Visibility { get; set; }

    /// <summary>
    /// Retries used to get information. Used only for ClickHouse.
    /// </summary>
    [NotMapped] public ushort RetriesUsed { get; set; }


    public char? Environment { get; set; }



    public bool? VAC { get; set; }

    public string? Keywords { get; set; }


    public ulong? SteamID { get; set; }


    public long? ASN { get; set; }

    public string? ISP { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Country { get; set; }

    public Continent? Continent { get; set; }

    public string? Timezone { get; set; }

    [Required] public bool IsOnline { get; set; }

    [Required] public bool ServerDead { get; set; }


    [Required] public DateTime LastCheck { get; set; }

    [Required] public DateTime NextCheck { get; set; }

    [Required] public int FailedChecks { get; set; }

    [Required] public DateTime FoundAt { get; set; }

    public void Copy(Server toCopy)
    {
        ServerID = toCopy.ServerID;
        Name = toCopy.Name;
        SearchVector = toCopy.SearchVector;
        Game = toCopy.Game;
        Map = toCopy.Map;
        AppID = toCopy.AppID;
        IpAddressBytes = toCopy.IpAddressBytes;
        Address = toCopy.Address;
        Port = toCopy.Port;
        QueryPort = toCopy.QueryPort;
        Players = toCopy.Players;
        MaxPlayers = toCopy.MaxPlayers;
        ASN = toCopy.ASN;
        ISP = toCopy.ISP;
        Latitude = toCopy.Latitude;
        Longitude = toCopy.Longitude;
        Country = toCopy.Country;
        Continent = toCopy.Continent;
        Timezone = toCopy.Timezone;
        IsOnline = toCopy.IsOnline;
        ServerDead = toCopy.ServerDead;
        LastCheck = toCopy.LastCheck;
        NextCheck = toCopy.NextCheck;
        FailedChecks = toCopy.FailedChecks;
        FoundAt = toCopy.FoundAt;
    }
}