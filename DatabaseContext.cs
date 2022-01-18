using HTB_Updates_Discord_Bot.Models;
using HTB_Updates_Discord_Bot.Models.Database;
using HTB_Updates_Discord_Bot.Models.Shared;
using Microsoft.EntityFrameworkCore;
using System;

namespace HTB_Updates_Discord_Bot
{
    public class DatabaseContext : DbContext
    {
        public DbSet<HTBUser> HTBUsers { get; set; }
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
