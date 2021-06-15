using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class PaymentTransaciont_UpayTransactionDetails_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpayTransactionDetails_CashieriD",
                table: "PaymentTransaction",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpayTransactionDetails_CreditCardCompanyCode",
                table: "PaymentTransaction",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpayTransactionDetails_ErrorDescription",
                table: "PaymentTransaction",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpayTransactionDetails_ErrorMessage",
                table: "PaymentTransaction",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpayTransactionDetails_MerchantNumber",
                table: "PaymentTransaction",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpayTransactionDetails_SessionID",
                table: "PaymentTransaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UpayTransactionDetails_TotalAmount",
                table: "PaymentTransaction",
                type: "decimal(19,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpayTransactionDetails_WebUrl",
                table: "PaymentTransaction",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpayTransactionDetails_CashieriD",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UpayTransactionDetails_CreditCardCompanyCode",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UpayTransactionDetails_ErrorDescription",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UpayTransactionDetails_ErrorMessage",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UpayTransactionDetails_MerchantNumber",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UpayTransactionDetails_SessionID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UpayTransactionDetails_TotalAmount",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "UpayTransactionDetails_WebUrl",
                table: "PaymentTransaction");
        }
    }
}
