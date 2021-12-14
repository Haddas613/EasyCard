using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class pinpad_correlation_id_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransaction_PinPadTransactionID",
                table: "PaymentTransaction");

            migrationBuilder.AddColumn<string>(
                name: "PinPadCorrelationID",
                table: "PaymentTransaction",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PinPadCorrelationID",
                table: "PaymentTransaction");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransaction_PinPadTransactionID",
                table: "PaymentTransaction",
                column: "PinPadTransactionID");
        }
    }
}
