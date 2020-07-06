using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class rejectionReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "FinalizationStatus",
                table: "PaymentTransaction",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionMessage",
                table: "PaymentTransaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistory_PaymentTransactionID",
                table: "TransactionHistory",
                column: "PaymentTransactionID");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionHistory_PaymentTransaction_PaymentTransactionID",
                table: "TransactionHistory",
                column: "PaymentTransactionID",
                principalTable: "PaymentTransaction",
                principalColumn: "PaymentTransactionID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionHistory_PaymentTransaction_PaymentTransactionID",
                table: "TransactionHistory");

            migrationBuilder.DropIndex(
                name: "IX_TransactionHistory_PaymentTransactionID",
                table: "TransactionHistory");

            migrationBuilder.DropColumn(
                name: "FinalizationStatus",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "RejectionMessage",
                table: "PaymentTransaction");
        }
    }
}
