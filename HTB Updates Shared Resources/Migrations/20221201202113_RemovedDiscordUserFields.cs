using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTB_Updates_Shared_Resources.Migrations
{
    public partial class RemovedDiscordUserFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_DiscordGuilds_GuildId",
                table: "HTBUpdates_DiscordUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_HTBUsers_HTBUserId",
                table: "HTBUpdates_DiscordUsers");

            migrationBuilder.DropIndex(
                name: "IX_HTBUpdates_DiscordUsers_GuildId",
                table: "HTBUpdates_DiscordUsers");

            migrationBuilder.DropIndex(
                name: "IX_HTBUpdates_DiscordUsers_HTBUserId",
                table: "HTBUpdates_DiscordUsers");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "HTBUpdates_DiscordUsers");

            migrationBuilder.DropColumn(
                name: "HTBUserId",
                table: "HTBUpdates_DiscordUsers");

            migrationBuilder.DropColumn(
                name: "Verified",
                table: "HTBUpdates_DiscordUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GuildId",
                table: "HTBUpdates_DiscordUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HTBUserId",
                table: "HTBUpdates_DiscordUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Verified",
                table: "HTBUpdates_DiscordUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_HTBUpdates_DiscordUsers_GuildId",
                table: "HTBUpdates_DiscordUsers",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_HTBUpdates_DiscordUsers_HTBUserId",
                table: "HTBUpdates_DiscordUsers",
                column: "HTBUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_DiscordGuilds_GuildId",
                table: "HTBUpdates_DiscordUsers",
                column: "GuildId",
                principalTable: "HTBUpdates_DiscordGuilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_HTBUsers_HTBUserId",
                table: "HTBUpdates_DiscordUsers",
                column: "HTBUserId",
                principalTable: "HTBUpdates_HTBUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
