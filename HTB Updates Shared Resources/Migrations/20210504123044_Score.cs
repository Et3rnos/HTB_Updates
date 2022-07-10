using Microsoft.EntityFrameworkCore.Migrations;

namespace HTB_Updates_Shared_Resources.Migrations
{
    public partial class Score : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "HTBUpdates_HTBUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "HTBUpdates_HTBUsers");
        }
    }
}
