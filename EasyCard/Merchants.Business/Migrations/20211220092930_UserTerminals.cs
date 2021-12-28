using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class UserTerminals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Terminals",
                table: "UserTerminalMapping",
                type: "varchar(max)",
                unicode: false,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Terminals",
                table: "UserTerminalMapping");
        }
    }
}
