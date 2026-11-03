using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireHub.Migrations
{
    public partial class AddInterviewDetailsToApplication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InterviewDetails",
                table: "Applications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterviewDetails",
                table: "Applications");
        }
    }
}
