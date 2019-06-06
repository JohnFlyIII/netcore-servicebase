using ServiceBase.OData.Core.Entities;
using ServiceBase.OData.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace ServiceBase.OData.Infrastructure.Data
{
    public class HeroesContext : DbContext
    {
        public HeroesContext(DbContextOptions<HeroesContext> options)
            : base (options)
        {
        }

        public DbSet<HeroDataModel> Heroes { get; set; }
        public DbSet<SidekickEntity> Sidekicks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
