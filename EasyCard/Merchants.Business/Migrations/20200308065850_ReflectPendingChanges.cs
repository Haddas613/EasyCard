using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class ReflectPendingChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TerminalID",
                table: "MerchantHistory",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MerchantHistory_TerminalID",
                table: "MerchantHistory",
                column: "TerminalID");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantHistory_Terminal_TerminalID",
                table: "MerchantHistory",
                column: "TerminalID",
                principalTable: "Terminal",
                principalColumn: "TerminalID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantHistory_Terminal_TerminalID",
                table: "MerchantHistory");

            migrationBuilder.DropIndex(
                name: "IX_MerchantHistory_TerminalID",
                table: "MerchantHistory");

            migrationBuilder.DropColumn(
                name: "TerminalID",
                table: "MerchantHistory");
        }
    }
}
