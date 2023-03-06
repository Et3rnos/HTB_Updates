using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTB_Updates_Shared_Resources.Migrations
{
    public partial class NoSupporter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HTBUpdates_Supporters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HTBUpdates_Supporters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Border = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DiscordId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Slogan = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HTBUpdates_Supporters", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
