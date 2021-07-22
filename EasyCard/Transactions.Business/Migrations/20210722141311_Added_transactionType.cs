using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class Added_transactionType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DealDetails_TransactionType",
                table: "PaymentTransaction",
                newName: "PaymentTransaction_TransactionType");

            migrationBuilder.RenameColumn(
                name: "DealDetails_TransactionType",
                table: "PaymentRequest",
                newName: "TransactionType");

            migrationBuilder.RenameColumn(
                name: "DealDetails_TransactionType",
                table: "BillingDeal",
                newName: "TransactionType");

            migrationBuilder.AlterColumn<short>(
                name: "TransactionType",
                table: "PaymentTransaction",
                type: "smallint",
                nullable: true,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<short>(
                name: "PaymentTransaction_TransactionType",
                table: "PaymentTransaction",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "TransactionType",
                table: "PaymentRequest",
                type: "smallint",
                nullable: true,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "TransactionType",
                table: "BillingDeal",
                type: "smallint",
                nullable: true,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentTransaction_TransactionType",
                table: "PaymentTransaction",
                newName: "DealDetails_TransactionType");

            migrationBuilder.RenameColumn(
                name: "TransactionType",
                table: "PaymentRequest",
                newName: "DealDetails_TransactionType");

            migrationBuilder.RenameColumn(
                name: "TransactionType",
                table: "BillingDeal",
                newName: "DealDetails_TransactionType");

            migrationBuilder.AlterColumn<short>(
                name: "TransactionType",
                table: "PaymentTransaction",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true,
                oldDefaultValue: (short)0);

            migrationBuilder.AlterColumn<short>(
                name: "DealDetails_TransactionType",
                table: "PaymentTransaction",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<short>(
                name: "DealDetails_TransactionType",
                table: "PaymentRequest",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true,
                oldDefaultValue: (short)0);

            migrationBuilder.AlterColumn<short>(
                name: "DealDetails_TransactionType",
                table: "BillingDeal",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true,
                oldDefaultValue: (short)0);
        }
    }
}
