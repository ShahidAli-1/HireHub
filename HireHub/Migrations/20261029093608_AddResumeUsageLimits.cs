using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireHub.Migrations
{
    public partial class AddResumeUsageLimits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResumeEditsToday",
                table: "DailyUsages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ResumeScoresToday",
                table: "DailyUsages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumeEditsToday",
                table: "DailyUsages");

            migrationBuilder.DropColumn(
                name: "ResumeScoresToday",
                table: "DailyUsages");
        }
    }
}
