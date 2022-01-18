/*using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace HTB_Updates_Discord_Bot
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseMySql(Program.CONNECTION_STRING,
                    new MySqlServerVersion(new Version(5, 7)),
                    x => x.MigrationsHistoryTable("HTBUpdates_EFMigrationsHistory"));

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}*/
