using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class J5RequiredFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "J2Allowed",
                table: "Terminal",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "J5Allowed",
                table: "Terminal",
                nullable: true,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "J2Allowed",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "J5Allowed",
                table: "Terminal");
        }
    }
}
