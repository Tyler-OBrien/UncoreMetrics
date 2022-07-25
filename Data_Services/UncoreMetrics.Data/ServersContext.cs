using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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

    public DbSet<GenericServer> Servers { get; set; }


    public DbSet<VRisingServer> VRisingServers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenericServer>().ToTable("Servers");
        modelBuilder.Entity<GenericServer>().HasIndex(server => server.AppID);
        modelBuilder.Entity<GenericServer>().HasIndex(server => server.Continent);
        modelBuilder.Entity<GenericServer>().HasIndex(server => server.NextCheck);
        modelBuilder.Entity<GenericServer>().HasIndex(server => server.ServerDead);
        modelBuilder.Entity<GenericServer>().HasIndex(server => server.IsOnline);
        modelBuilder.Entity<GenericServer>()
            .HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "english", // Text search config
                p => new { p.Name }) // Included properties
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)

        modelBuilder.Entity<GenericServer>().HasKey(server => new { server.IpAddressBytes, server.Port });
        modelBuilder.Entity<GenericServer>().HasKey(server => server.ServerID);
        modelBuilder.Entity<GenericServer>().Property(p => p.SearchVector).ValueGeneratedOnAdd()
            .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);


        modelBuilder.Entity<VRisingServer>().ToTable("V_Rising_Servers");
        modelBuilder.Entity<VRisingServer>().HasIndex(server => server.HeartDamage);
        modelBuilder.Entity<VRisingServer>().HasIndex(server => server.BloodBoundEquipment);
    }
}