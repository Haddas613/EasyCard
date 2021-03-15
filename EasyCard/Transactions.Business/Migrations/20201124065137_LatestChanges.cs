using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class LatestChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "BillingDeal");

            migrationBuilder.AddColumn<short>(
                name: "PaymentTypeEnum",
                table: "PaymentTransaction",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentTypeEnum",
                table: "PaymentTransaction");

            migrationBuilder.AddColumn<short>(
                name: "Status",
                table: "BillingDeal",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
