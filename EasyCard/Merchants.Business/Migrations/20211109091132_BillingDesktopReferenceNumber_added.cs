using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class BillingDesktopReferenceNumber_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingDesktopRefNumber",
                table: "Item",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingDesktopRefNumber",
                table: "Consumer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingDesktopRefNumber",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "BillingDesktopRefNumber",
                table: "Consumer");
        }
    }
}
