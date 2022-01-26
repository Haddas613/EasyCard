using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class PaymentTransaction_BitColumns_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BitPaymentInitiationId",
                table: "PaymentTransaction",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BitTransactionSerialId",
                table: "PaymentTransaction",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BitPaymentInitiationId",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "BitTransactionSerialId",
                table: "PaymentTransaction");
        }
    }
}
