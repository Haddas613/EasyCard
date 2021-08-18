using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class Added_externalconsumercode_N_solek : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalConsumerCode",
                table: "PaymentTransaction",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalConsumerCode",
                table: "PaymentRequest",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalConsumerCode",
                table: "Invoice",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Solek",
                table: "Invoice",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Solek",
                table: "CreditCardTokenDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalConsumerCode",
                table: "BillingDeal",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Solek",
                table: "BillingDeal",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalConsumerCode",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ExternalConsumerCode",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "ExternalConsumerCode",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "Solek",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "Solek",
                table: "CreditCardTokenDetails");

            migrationBuilder.DropColumn(
                name: "ExternalConsumerCode",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "Solek",
                table: "BillingDeal");
        }
    }
}
