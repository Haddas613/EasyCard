using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class ConsumerName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsumerName",
                table: "PaymentTransaction",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerName",
                table: "PaymentRequest",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerName",
                table: "Invoice",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerName",
                table: "BillingDeal",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumerName",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ConsumerName",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "ConsumerName",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "ConsumerName",
                table: "BillingDeal");
        }
    }
}
