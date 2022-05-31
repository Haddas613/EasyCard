using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class NetDiscountTotal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NetDiscountTotal",
                table: "PaymentTransaction",
                type: "decimal(19,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UserAmount",
                table: "PaymentTransaction",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetDiscountTotal",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UserAmount",
                table: "PaymentTransaction");
        }
    }
}
