using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class v10_isalien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAlien",
                table: "User",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAlien",
                table: "User");
        }
    }
}
