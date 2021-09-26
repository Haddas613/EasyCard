using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class CardTokensIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CreditCardTokenDetails_Active_MerchantID_ConsumerID",
                table: "CreditCardTokenDetails",
                columns: new[] { "Active", "MerchantID", "ConsumerID" });

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardTokenDetails_Active_TerminalID_ConsumerID",
                table: "CreditCardTokenDetails",
                columns: new[] { "Active", "TerminalID", "ConsumerID" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CreditCardTokenDetails_Active_MerchantID_ConsumerID",
                table: "CreditCardTokenDetails");

            migrationBuilder.DropIndex(
                name: "IX_CreditCardTokenDetails_Active_TerminalID_ConsumerID",
                table: "CreditCardTokenDetails");
        }
    }
}
