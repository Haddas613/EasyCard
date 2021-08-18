using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class BankDetailsCorrections2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankDetails_PaymentType",
                table: "BillingDeal");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "BankDetails_PaymentType",
                table: "BillingDeal",
                type: "smallint",
                nullable: true);
        }
    }
}
