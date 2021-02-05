using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class Terminal_AggregatorAndProcessorReferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AggregatorTerminalReference",
                table: "Terminal",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessorTerminalReference",
                table: "Terminal",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AggregatorTerminalReference",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "ProcessorTerminalReference",
                table: "Terminal");
        }
    }
}
