using HTB_Updates_Shared_Resources.Models;
using HTB_Updates_Shared_Resources.Models.Database;
using HTB_Updates_Shared_Resources.Models.Shared;
using Microsoft.EntityFrameworkCore;
using System;

namespace HTB_Updates_Shared_Resources
{
    public class DatabaseContext : DbContext
    {
        public DbSet<HTBUser> HTBUsers { get; set; }
        public DbSet<GuildUser> GuildUsers { get; set; }
        public DbSet<DiscordUser> DiscordUsers { get; set; }
        public DbSet<DiscordGuild> DiscordGuilds { get; set; }
        public DbSet<Solve> Solves { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                entityType.SetTableName($"HTBUpdates_{tableName}");
            }
        }
    }
}
