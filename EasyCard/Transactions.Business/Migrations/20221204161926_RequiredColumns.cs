using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class RequiredColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailRequired",
                table: "PaymentRequest",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NationalIDRequired",
                table: "PaymentRequest",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneRequired",
                table: "PaymentRequest",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailRequired",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "NationalIDRequired",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "PhoneRequired",
                table: "PaymentRequest");
        }
    }
}
