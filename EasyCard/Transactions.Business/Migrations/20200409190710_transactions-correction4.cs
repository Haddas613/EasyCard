using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class transactionscorrection4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntegrationMessageId",
                table: "TransactionHistory");

            migrationBuilder.AddColumn<int>(
                name: "CurrentDeal",
                table: "PaymentTransaction",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "JDealType",
                table: "PaymentTransaction",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "SpecialTransactionType",
                table: "PaymentTransaction",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Solek",
                table: "PaymentTransaction",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentDeal",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "JDealType",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "SpecialTransactionType",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "Solek",
                table: "PaymentTransaction");

            migrationBuilder.AddColumn<string>(
                name: "IntegrationMessageId",
                table: "TransactionHistory",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }
    }
}
