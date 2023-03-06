using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTB_Updates_Shared_Resources.Migrations
{
    public partial class FinalGuildMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HTBUpdates_GuildUsers_HTBUpdates_DiscordUsers_DiscordUserId",
                table: "HTBUpdates_GuildUsers");

            migrationBuilder.DropColumn(
                name: "DiscordId",
                table: "HTBUpdates_GuildUsers");

            migrationBuilder.AlterColumn<int>(
                name: "DiscordUserId",
                table: "HTBUpdates_GuildUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HTBUpdates_GuildUsers_HTBUpdates_DiscordUsers_DiscordUserId",
                table: "HTBUpdates_GuildUsers",
                column: "DiscordUserId",
                principalTable: "HTBUpdates_DiscordUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HTBUpdates_GuildUsers_HTBUpdates_DiscordUsers_DiscordUserId",
                table: "HTBUpdates_GuildUsers");

            migrationBuilder.AlterColumn<int>(
                name: "DiscordUserId",
                table: "HTBUpdates_GuildUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<ulong>(
                name: "DiscordId",
                table: "HTBUpdates_GuildUsers",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddForeignKey(
                name: "FK_HTBUpdates_GuildUsers_HTBUpdates_DiscordUsers_DiscordUserId",
                table: "HTBUpdates_GuildUsers",
                column: "DiscordUserId",
                principalTable: "HTBUpdates_DiscordUsers",
                principalColumn: "Id");
        }
    }
}
