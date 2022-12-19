using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UncoreMetrics.Data.Discord
{
    public class DiscordContext : DbContext
    {

        public DiscordContext(DbContextOptions<DiscordContext> contextOptions)
            : base(contextOptions)
        {
        }

        protected DiscordContext(DbContextOptions contextOptions)
            : base(contextOptions)
        {
        }


        public DbSet<DiscordChannelLink> Links { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiscordChannelLink>().ToTable("Discord_Links");
            modelBuilder.Entity<DiscordChannelLink>().HasKey(link => link.ID);
            modelBuilder.Entity<DiscordChannelLink>().HasIndex(link => link.ServerID);
            modelBuilder.Entity<DiscordChannelLink>().HasIndex(link => link.UserID);
            modelBuilder.Entity<DiscordChannelLink>().HasIndex(link => link.GameServerID);
            modelBuilder.Entity<DiscordChannelLink>().HasIndex(link => link.ChannelID);
            modelBuilder.Entity<DiscordChannelLink>().HasIndex(link => link.Enabled);
        }
    }
}
