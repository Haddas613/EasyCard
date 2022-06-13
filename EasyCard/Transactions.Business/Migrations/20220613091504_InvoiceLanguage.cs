using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class InvoiceLanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoiceLanguage",
                table: "PaymentRequest",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceLanguage",
                table: "Invoice",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceLanguage",
                table: "BillingDeal",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceLanguage",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "InvoiceLanguage",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InvoiceLanguage",
                table: "BillingDeal");
        }
    }
}
