using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTB_Updates_Shared_Resources.Migrations
{
    public partial class RemovedDiscordUserFieldsSql : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into HTBUpdates_DiscordUsers (DiscordId) select distinct DiscordId from HTBUpdates_GuildUsers");
            migrationBuilder.Sql("update HTBUpdates_GuildUsers inner join HTBUpdates_DiscordUsers on HTBUpdates_GuildUsers.DiscordId = HTBUpdates_DiscordUsers.DiscordId set HTBUpdates_GuildUsers.DiscordUserId = HTBUpdates_DiscordUsers.Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
