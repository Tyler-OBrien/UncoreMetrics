using Microsoft.EntityFrameworkCore;

namespace UncoreMetrics.Data;

public class GenericServersContext : DbContext
{
    public GenericServersContext(DbContextOptions<GenericServersContext> contextOptions)
        : base(contextOptions)
    {
    }

    protected GenericServersContext(DbContextOptions contextOptions)
        : base(contextOptions)
    {
    }

    public DbSet<GenericServer> Servers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenericServer>().ToTable("Servers");
        modelBuilder.Entity<GenericServer>().HasIndex(server => server.AppID);
        modelBuilder.Entity<GenericServer>().HasIndex(server => server.Continent);
        modelBuilder.Entity<GenericServer>().HasIndex(server => server.IsOnline);
        modelBuilder.Entity<GenericServer>()
            .HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "english", // Text search config
                p => new { p.Name }) // Included properties
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)

        modelBuilder.Entity<GenericServer>().HasKey(server => new { server.IpAddressBytes, server.Port });
    }
}