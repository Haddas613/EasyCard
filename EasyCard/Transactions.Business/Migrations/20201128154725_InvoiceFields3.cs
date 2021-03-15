using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class InvoiceFields3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardExpiration",
                table: "Invoice",
                type: "varchar(5)",
                unicode: false,
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "Invoice",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardVendor",
                table: "Invoice",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardExpiration",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "CardVendor",
                table: "Invoice");
        }
    }
}
