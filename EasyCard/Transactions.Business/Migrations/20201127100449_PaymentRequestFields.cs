using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class PaymentRequestFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InitialPaymentAmount",
                table: "PaymentRequest",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InstallmentPaymentAmount",
                table: "PaymentRequest",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "PaymentRequest",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialPaymentAmount",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "InstallmentPaymentAmount",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "PaymentRequest");
        }
    }
}
