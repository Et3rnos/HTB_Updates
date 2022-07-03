using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTB_Updates_Discord_Bot.Migrations
{
    public partial class optional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OptionalAnnouncements",
                table: "HTBUpdates_DiscordGuilds",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptionalAnnouncements",
                table: "HTBUpdates_DiscordGuilds");
        }
    }
}
