using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class upay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "UpayCreditCardCompanyCode",
                table: "PaymentTransaction",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpayMerchantNumber",
                table: "PaymentTransaction",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpayTransactionID",
                table: "PaymentTransaction",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpayWebUrl",
                table: "PaymentTransaction",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "Invoice",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "CreditCardTokenDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "BillingDeal",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ShvaAuthNum",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UpayCreditCardCompanyCode",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UpayMerchantNumber",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UpayTransactionID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UpayWebUrl",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "CreditCardTokenDetails");

            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "BillingDeal");
        }
    }
}
