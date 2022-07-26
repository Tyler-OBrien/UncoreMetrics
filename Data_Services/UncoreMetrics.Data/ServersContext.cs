using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using UncoreMetrics.Data.GameData.ARK;
using UncoreMetrics.Data.GameData.VRising;

namespace UncoreMetrics.Data;

public class ServersContext : DbContext
{
    public ServersContext(DbContextOptions<ServersContext> contextOptions)
        : base(contextOptions)
    {
    }

    protected ServersContext(DbContextOptions contextOptions)
        : base(contextOptions)
    {
    }

    public DbSet<Server> Servers { get; set; }


    public DbSet<VRisingServer> VRisingServers { get; set; }



    public DbSet<ArkServer> ArkServer { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Server>().ToTable("Servers");
        modelBuilder.Entity<Server>().HasIndex(server => server.AppID);
        modelBuilder.Entity<Server>().HasIndex(server => server.Continent);
        modelBuilder.Entity<Server>().HasIndex(server => server.NextCheck);
        modelBuilder.Entity<Server>().HasIndex(server => server.ServerDead);
        modelBuilder.Entity<Server>().HasIndex(server => server.IsOnline);
        modelBuilder.Entity<Server>()
            .HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "english", // Text search config
                p => new { p.Name }) // Included properties
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)

        modelBuilder.Entity<Server>().HasKey(server => new { server.IpAddressBytes, server.Port });
        modelBuilder.Entity<Server>().HasKey(server => server.ServerID);
        modelBuilder.Entity<Server>().Property(p => p.SearchVector).ValueGeneratedOnAdd()
            .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);


        modelBuilder.Entity<VRisingServer>().ToTable("V_Rising_Servers");
        modelBuilder.Entity<VRisingServer>().HasIndex(server => server.HeartDamage);
        modelBuilder.Entity<VRisingServer>().HasIndex(server => server.BloodBoundEquipment);

        modelBuilder.Entity<ArkServer>().ToTable("Ark_Servers");
        modelBuilder.Entity<ArkServer>().HasIndex(server => server.Battleye);
        modelBuilder.Entity<ArkServer>().HasIndex(server => server.PVE);
        modelBuilder.Entity<ArkServer>().HasIndex(server => server.PasswordRequired);
    }
}