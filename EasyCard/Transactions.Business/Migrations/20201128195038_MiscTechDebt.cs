using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class MiscTechDebt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "DocumentOrigin",
                table: "PaymentTransaction",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "PaymentRequest",
                type: "decimal(19,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InstallmentPaymentAmount",
                table: "PaymentRequest",
                type: "decimal(19,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialPaymentAmount",
                table: "PaymentRequest",
                type: "decimal(19,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<short>(
                name: "DocumentOrigin",
                table: "PaymentRequest",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Invoice",
                type: "decimal(19,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InstallmentPaymentAmount",
                table: "Invoice",
                type: "decimal(19,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialPaymentAmount",
                table: "Invoice",
                type: "decimal(19,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<short>(
                name: "DocumentOrigin",
                table: "Invoice",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "DocumentOrigin",
                table: "CreditCardTokenDetails",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "DocumentOrigin",
                table: "BillingDeal",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentOrigin",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "DocumentOrigin",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "DocumentOrigin",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "DocumentOrigin",
                table: "CreditCardTokenDetails");

            migrationBuilder.DropColumn(
                name: "DocumentOrigin",
                table: "BillingDeal");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "PaymentRequest",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InstallmentPaymentAmount",
                table: "PaymentRequest",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialPaymentAmount",
                table: "PaymentRequest",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Invoice",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InstallmentPaymentAmount",
                table: "Invoice",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialPaymentAmount",
                table: "Invoice",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,4)");
        }
    }
}
