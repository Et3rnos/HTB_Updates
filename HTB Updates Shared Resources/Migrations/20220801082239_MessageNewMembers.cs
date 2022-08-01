using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTB_Updates_Shared_Resources.Migrations
{
    public partial class MessageNewMembers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MessageNewMembers",
                table: "HTBUpdates_DiscordGuilds",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageNewMembers",
                table: "HTBUpdates_DiscordGuilds");
        }
    }
}
