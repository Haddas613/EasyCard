using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class InvoiceDetails_Donation_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InvoiceDetails_Donation",
                table: "PaymentRequest",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InvoiceDetails_Donation",
                table: "Invoice",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InvoiceDetails_Donation",
                table: "BillingDeal",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceDetails_Donation",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "InvoiceDetails_Donation",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InvoiceDetails_Donation",
                table: "BillingDeal");
        }
    }
}
