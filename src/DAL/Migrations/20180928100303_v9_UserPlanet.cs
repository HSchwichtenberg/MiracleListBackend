using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class v9_UserPlanet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Planet",
                table: "User",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Planet",
                table: "User");
        }
    }
}
