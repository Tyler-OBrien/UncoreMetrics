using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using UncoreMetrics.Data.GameData._7DaysToDie;
using UncoreMetrics.Data.GameData.ARK;
using UncoreMetrics.Data.GameData.Arma3;
using UncoreMetrics.Data.GameData.DayZ;
using UncoreMetrics.Data.GameData.HellLetLoose;
using UncoreMetrics.Data.GameData.PostScriptum;
using UncoreMetrics.Data.GameData.ProjectZomboid;
using UncoreMetrics.Data.GameData.Rust;
using UncoreMetrics.Data.GameData.Unturned;
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

    public DbSet<ScrapeJob> ScrapeJobs { get; set; }

    public DbSet<SevenDaysToDieServer> SevenDaysToDieServers { get; set; }

    public DbSet<ArkServer> ArkServers { get; set; }

    public DbSet<Arma3Server> Arma3Servers { get; set; }


    public DbSet<DayZServer> DayZServers { get; set; }


    public DbSet<HellLetLooseServer> HellLetLooseServers { get; set; }


    public DbSet<PostScriptumServer> PostScriptumServers { get; set; }


    public DbSet<ProjectZomboidServer> ProjectZomboidServers { get; set; }


    public DbSet<RustServer> RustServers { get; set; }


    public DbSet<UnturnedServer> UnturnedServers { get; set; }


    public DbSet<VRisingServer> VRisingServers { get; set; }


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
        modelBuilder.Entity<Server>().HasIndex(server => server.Players);
        modelBuilder.Entity<Server>().HasIndex(server => server.Address);
        modelBuilder.Entity<Server>()
            .HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "english", // Text search config
                p => new { p.Name }) // Included properties
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)


        modelBuilder.Entity<Server>().HasKey(server => server.ServerID);
        modelBuilder.Entity<Server>().HasIndex(server => new { server.IpAddressBytes, server.QueryPort }).IsUnique();
        modelBuilder.Entity<Server>().Property(p => p.SearchVector).ValueGeneratedOnAdd()
            .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

        modelBuilder.Entity<SevenDaysToDieServer>().ToTable("SevenDTD_Servers");
        modelBuilder.Entity<SevenDaysToDieServer>().HasIndex(server => server.EACEnabled);
        modelBuilder.Entity<SevenDaysToDieServer>().HasIndex(server => server.IsPasswordProtected);
        modelBuilder.Entity<SevenDaysToDieServer>().HasIndex(server => server.DayCount);
        modelBuilder.Entity<SevenDaysToDieServer>().HasIndex(server => server.RequiresMod);
        modelBuilder.Entity<SevenDaysToDieServer>().HasIndex(server => server.Version);


        modelBuilder.Entity<ArkServer>().ToTable("Ark_Servers");
        modelBuilder.Entity<ArkServer>().HasIndex(server => server.Battleye);
        modelBuilder.Entity<ArkServer>().HasIndex(server => server.PVE);
        modelBuilder.Entity<ArkServer>().HasIndex(server => server.PasswordRequired);


        modelBuilder.Entity<Arma3Server>().ToTable("Arma3_Servers");


        modelBuilder.Entity<DayZServer>().ToTable("DayZ_Servers");
        modelBuilder.Entity<DayZServer>().HasIndex(server => server.Island);
        modelBuilder.Entity<DayZServer>().HasIndex(server => server.AllowedBuild);


        modelBuilder.Entity<HellLetLooseServer>().ToTable("HellLetLoose_Servers");
        modelBuilder.Entity<HellLetLooseServer>().HasIndex(server => server.Visible);


        modelBuilder.Entity<PostScriptumServer>().ToTable("PostScriptum_Servers");
        modelBuilder.Entity<PostScriptumServer>().HasIndex(server => server.CurrentModLoadedCount);
        modelBuilder.Entity<PostScriptumServer>().HasIndex(server => server.GameMode);

        modelBuilder.Entity<ProjectZomboidServer>().ToTable("ProjectZomboid_Servers");
        modelBuilder.Entity<ProjectZomboidServer>().HasIndex(server => server.Open);
        modelBuilder.Entity<ProjectZomboidServer>().HasIndex(server => server.PvP);

        modelBuilder.Entity<RustServer>().ToTable("Rust_Servers");
        modelBuilder.Entity<RustServer>().HasIndex(server => server.PvE);
        modelBuilder.Entity<RustServer>().HasIndex(server => server.EntityCount);


        modelBuilder.Entity<UnturnedServer>().ToTable("Unturned_Servers");
        modelBuilder.Entity<UnturnedServer>().HasIndex(server => server.Mods);


        modelBuilder.Entity<VRisingServer>().ToTable("V_Rising_Servers");
        modelBuilder.Entity<VRisingServer>().HasIndex(server => server.HeartDamage);
        modelBuilder.Entity<VRisingServer>().HasIndex(server => server.BloodBoundEquipment);


        modelBuilder.Entity<ScrapeJob>().ToTable("Scrape_Jobs");
        modelBuilder.Entity<ScrapeJob>().HasKey(job => job.InternalId);
    }
}