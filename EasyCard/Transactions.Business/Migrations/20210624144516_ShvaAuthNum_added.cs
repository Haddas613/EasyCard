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
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "PaymentTransaction",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShvaAuthNum",
                table: "PaymentTransaction",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardDetails_CardBrand",
                table: "Invoice",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "CreditCardTokenDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardDetails_CardBrand",
                table: "BillingDeal",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditCardDetails_CardBrand",
                table: "vFutureBillings");

            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ShvaAuthNum",
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
