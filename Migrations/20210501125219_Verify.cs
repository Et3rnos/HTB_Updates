using Microsoft.EntityFrameworkCore.Migrations;

namespace HTB_Updates_Discord_Bot.Migrations
{
    public partial class Verify : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Verified",
                table: "HTBUpdates_DiscordUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Verified",
                table: "HTBUpdates_DiscordUsers");
        }
    }
}
