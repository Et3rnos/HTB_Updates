using Microsoft.EntityFrameworkCore.Migrations;

namespace HTB_Updates_Discord_Bot.Migrations
{
    public partial class QuickFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_DiscordGuilds_GuildId",
                table: "HTBUpdates_DiscordUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_HTBUsers_HTBUserId",
                table: "HTBUpdates_DiscordUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_HTBUpdates_Solves_HTBUpdates_HTBUsers_HTBUserId",
                table: "HTBUpdates_Solves");

            migrationBuilder.AlterColumn<int>(
                name: "HTBUserId",
                table: "HTBUpdates_Solves",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HTBUserId",
                table: "HTBUpdates_DiscordUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GuildId",
                table: "HTBUpdates_DiscordUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_HTBUpdates_Solves_HTBUpdates_HTBUsers_HTBUserId",
                table: "HTBUpdates_Solves",
                column: "HTBUserId",
                principalTable: "HTBUpdates_HTBUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_DiscordGuilds_GuildId",
                table: "HTBUpdates_DiscordUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_HTBUsers_HTBUserId",
                table: "HTBUpdates_DiscordUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_HTBUpdates_Solves_HTBUpdates_HTBUsers_HTBUserId",
                table: "HTBUpdates_Solves");

            migrationBuilder.AlterColumn<int>(
                name: "HTBUserId",
                table: "HTBUpdates_Solves",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "HTBUserId",
                table: "HTBUpdates_DiscordUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "GuildId",
                table: "HTBUpdates_DiscordUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_DiscordGuilds_GuildId",
                table: "HTBUpdates_DiscordUsers",
                column: "GuildId",
                principalTable: "HTBUpdates_DiscordGuilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_HTBUsers_HTBUserId",
                table: "HTBUpdates_DiscordUsers",
                column: "HTBUserId",
                principalTable: "HTBUpdates_HTBUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HTBUpdates_Solves_HTBUpdates_HTBUsers_HTBUserId",
                table: "HTBUpdates_Solves",
                column: "HTBUserId",
                principalTable: "HTBUpdates_HTBUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
