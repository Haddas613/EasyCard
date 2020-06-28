using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class TerminalUserMappingsRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "UserTerminalMapping",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "UserTerminalMapping",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Roles",
                table: "UserTerminalMapping",
                unicode: false,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "UserTerminalMapping");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "UserTerminalMapping");

            migrationBuilder.DropColumn(
                name: "Roles",
                table: "UserTerminalMapping");
        }
    }
}
