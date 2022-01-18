using Microsoft.EntityFrameworkCore.Migrations;

namespace HTB_Updates_Discord_Bot.Migrations
{
    public partial class ChallCat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChallengeCategory",
                table: "HTBUpdates_Solves",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChallengeCategory",
                table: "HTBUpdates_Solves");
        }
    }
}
