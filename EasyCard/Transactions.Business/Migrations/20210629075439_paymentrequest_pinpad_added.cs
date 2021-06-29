using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class paymentrequest_pinpad_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PinPadTerminalID",
                table: "PaymentRequest",
                type: "varchar(16)",
                unicode: false,
                maxLength: 16,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PinPadTerminalID",
                table: "PaymentRequest");
        }
    }
}
