using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared_Collectors.Databases.Entities;

namespace Shared_Collectors.Databases.DataAccess
{
    public class GenericServersContext : DbContext
    {

        public GenericServersContext(DbContextOptions<GenericServersContext> options) : base(options)
        {
        }

        public DbSet<GenericServer> Servers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GenericServer>().ToTable("Servers");
        }
    }
}
