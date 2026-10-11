using System.Net;
using Microsoft.EntityFrameworkCore;
using UncoreMetrics.Data;

namespace UncoreMetrics.API.Tests;

/// <summary>
///     <see cref="ServersContext" /> adjusted for the InMemory provider, which can't map Postgres-only types.
///     Full-text search (<see cref="Server.SearchVector" />) isn't available, so search endpoints aren't covered here.
/// </summary>
public class InMemoryServersContext(DbContextOptions<InMemoryServersContext> options) : ServersContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Server>().Ignore(server => server.SearchVector);
        modelBuilder.Entity<Server>().Property(server => server.Address)
            .HasConversion(address => address.ToString(), value => IPAddress.Parse(value));
    }
}
