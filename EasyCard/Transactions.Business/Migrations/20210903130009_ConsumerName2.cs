using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class ConsumerName2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsumerNationalID",
                table: "PaymentTransaction",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerNationalID",
                table: "PaymentRequest",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerNationalID",
                table: "Invoice",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerNationalID",
                table: "BillingDeal",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumerNationalID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ConsumerNationalID",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "ConsumerNationalID",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "ConsumerNationalID",
                table: "BillingDeal");
        }
    }
}
