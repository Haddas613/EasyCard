using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class RefundAmount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BitMerchantNumber",
                table: "PaymentTransaction",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BitRequestStatusCode",
                table: "PaymentTransaction",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BitRequestStatusDescription",
                table: "PaymentTransaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRefund",
                table: "PaymentTransaction",
                type: "decimal(19,4)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BitMerchantNumber",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "BitRequestStatusCode",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "BitRequestStatusDescription",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "TotalRefund",
                table: "PaymentTransaction");
        }
    }
}
