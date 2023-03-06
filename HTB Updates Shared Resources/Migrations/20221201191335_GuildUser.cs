using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTB_Updates_Shared_Resources.Migrations
{
    public partial class GuildUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HTBUpdates_GuildUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DiscordId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GuildId = table.Column<int>(type: "int", nullable: false),
                    HTBUserId = table.Column<int>(type: "int", nullable: false),
                    DiscordUserId = table.Column<int>(type: "int", nullable: false),
                    Verified = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HTBUpdates_GuildUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HTBUpdates_GuildUsers_HTBUpdates_DiscordGuilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "HTBUpdates_DiscordGuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HTBUpdates_GuildUsers_HTBUpdates_DiscordUsers_DiscordUserId",
                        column: x => x.DiscordUserId,
                        principalTable: "HTBUpdates_DiscordUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HTBUpdates_GuildUsers_HTBUpdates_HTBUsers_HTBUserId",
                        column: x => x.HTBUserId,
                        principalTable: "HTBUpdates_HTBUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_HTBUpdates_GuildUsers_DiscordUserId",
                table: "HTBUpdates_GuildUsers",
                column: "DiscordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HTBUpdates_GuildUsers_GuildId",
                table: "HTBUpdates_GuildUsers",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_HTBUpdates_GuildUsers_HTBUserId",
                table: "HTBUpdates_GuildUsers",
                column: "HTBUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HTBUpdates_GuildUsers");
        }
    }
}
