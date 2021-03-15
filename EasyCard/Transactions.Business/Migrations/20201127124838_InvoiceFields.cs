using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class InvoiceFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InitialPaymentAmount",
                table: "Invoice",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InstallmentPaymentAmount",
                table: "Invoice",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Invoice",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialPaymentAmount",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InstallmentPaymentAmount",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Invoice");
        }
    }
}
