using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class PaymentTransaction_AggregatorAndProcessor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreditCardDetails_CardOwnerNationalId",
                table: "PaymentTransaction",
                newName: "CreditCardDetails_CardOwnerNationalID");

            migrationBuilder.AlterColumn<int>(
                name: "TransactionType",
                table: "PaymentTransaction",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<bool>(
                name: "CreditCardDetails_IsTourist",
                table: "PaymentTransaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AggregatorTerminalID",
                table: "PaymentTransaction",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessorTerminalID",
                table: "PaymentTransaction",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBin",
                table: "CreditCardTokenDetails",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditCardDetails_IsTourist",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "AggregatorTerminalID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ProcessorTerminalID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "CardBin",
                table: "CreditCardTokenDetails");

            migrationBuilder.RenameColumn(
                name: "CreditCardDetails_CardOwnerNationalID",
                table: "PaymentTransaction",
                newName: "CreditCardDetails_CardOwnerNationalId");

            migrationBuilder.AlterColumn<short>(
                name: "TransactionType",
                table: "PaymentTransaction",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
