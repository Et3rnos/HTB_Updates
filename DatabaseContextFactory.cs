using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HTB_Updates_Discord_Bot
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
            .AddJsonFile("appsettings.json", false)
            .Build();

            var connectionString = configuration.GetValue<string>("ConnectionString");

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseMySql(connectionString,
                    MySqlServerVersion.AutoDetect(connectionString),
                    x => x.MigrationsHistoryTable("HTBUpdates_EFMigrationsHistory"));

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
