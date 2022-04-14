using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class WoocommerceID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsumerEcwidID",
                table: "PaymentTransaction",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerWoocommerceID",
                table: "PaymentTransaction",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerEcwidID",
                table: "PaymentRequest",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerWoocommerceID",
                table: "PaymentRequest",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerEcwidID",
                table: "Invoice",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerWoocommerceID",
                table: "Invoice",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerEcwidID",
                table: "BillingDeal",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerWoocommerceID",
                table: "BillingDeal",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumerEcwidID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ConsumerWoocommerceID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ConsumerEcwidID",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "ConsumerWoocommerceID",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "ConsumerEcwidID",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "ConsumerWoocommerceID",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "ConsumerEcwidID",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "ConsumerWoocommerceID",
                table: "BillingDeal");
        }
    }
}
