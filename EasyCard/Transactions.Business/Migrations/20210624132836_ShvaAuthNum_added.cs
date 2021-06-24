using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class ShvaAuthNum_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreditCardDetails_CardBrand",
                table: "vFutureBillings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardDetails_CardBrand",
                table: "PaymentTransaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShvaTransactionDetails_ShvaAuthNum",
                table: "PaymentTransaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardDetails_CardBrand",
                table: "Invoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "CreditCardTokenDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardDetails_CardBrand",
                table: "BillingDeal",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditCardDetails_CardBrand",
                table: "vFutureBillings");

            migrationBuilder.DropColumn(
                name: "CreditCardDetails_CardBrand",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ShvaTransactionDetails_ShvaAuthNum",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "CreditCardDetails_CardBrand",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "CreditCardTokenDetails");

            migrationBuilder.DropColumn(
                name: "CreditCardDetails_CardBrand",
                table: "BillingDeal");
        }
    }
}
