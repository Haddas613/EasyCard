using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class PaymentRequestSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowCredit",
                table: "PaymentRequest",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowInstallments",
                table: "PaymentRequest",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HideEmail",
                table: "PaymentRequest",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HideNationalID",
                table: "PaymentRequest",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HidePhone",
                table: "PaymentRequest",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowAuthCode",
                table: "PaymentRequest",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowCredit",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "AllowInstallments",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "HideEmail",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "HideNationalID",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "HidePhone",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "ShowAuthCode",
                table: "PaymentRequest");
        }
    }
}
