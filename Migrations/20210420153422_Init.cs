using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HTB_Updates_Discord_Bot.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HTBUpdates_DiscordGuilds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HTBUpdates_DiscordGuilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HTBUpdates_HTBUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HtbId = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HTBUpdates_HTBUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HTBUpdates_DiscordUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DiscordId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GuildId = table.Column<int>(type: "int", nullable: true),
                    HTBUserId = table.Column<int>(type: "int", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HTBUpdates_DiscordUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_DiscordGuilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "HTBUpdates_DiscordGuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HTBUpdates_DiscordUsers_HTBUpdates_HTBUsers_HTBUserId",
                        column: x => x.HTBUserId,
                        principalTable: "HTBUpdates_HTBUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HTBUpdates_Solves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateDiff = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    ObjectType = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Type = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    FirstBlood = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SolveId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Points = table.Column<int>(type: "int", nullable: false),
                    MachineAvatar = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    HTBUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HTBUpdates_Solves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HTBUpdates_Solves_HTBUpdates_HTBUsers_HTBUserId",
                        column: x => x.HTBUserId,
                        principalTable: "HTBUpdates_HTBUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HTBUpdates_DiscordUsers_GuildId",
                table: "HTBUpdates_DiscordUsers",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_HTBUpdates_DiscordUsers_HTBUserId",
                table: "HTBUpdates_DiscordUsers",
                column: "HTBUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HTBUpdates_Solves_HTBUserId",
                table: "HTBUpdates_Solves",
                column: "HTBUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HTBUpdates_DiscordUsers");

            migrationBuilder.DropTable(
                name: "HTBUpdates_Solves");

            migrationBuilder.DropTable(
                name: "HTBUpdates_DiscordGuilds");

            migrationBuilder.DropTable(
                name: "HTBUpdates_HTBUsers");
        }
    }
}
