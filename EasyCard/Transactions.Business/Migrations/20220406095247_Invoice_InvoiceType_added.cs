using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class Invoice_InvoiceType_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "InvoiceBillingType",
                table: "Invoice",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceBillingType",
                table: "Invoice");
        }
    }
}
