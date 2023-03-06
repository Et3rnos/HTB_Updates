using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTB_Updates_Shared_Resources.Migrations
{
    public partial class GuildUserSql : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into HTBUpdates_GuildUsers (DiscordId, GuildId, HTBUserId, Verified) select DiscordId, GuildId, HTBUserId, Verified from HTBUpdates_DiscordUsers");
            migrationBuilder.Sql("delete from HTBUpdates_DiscordUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
