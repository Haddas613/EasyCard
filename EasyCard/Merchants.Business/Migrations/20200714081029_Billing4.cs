using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class Billing4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consumer_Merchant_MerchantID",
                table: "Consumer");

            migrationBuilder.DropIndex(
                name: "IX_Consumer_MerchantID",
                table: "Consumer");

            migrationBuilder.AddColumn<Guid>(
                name: "TerminalID",
                table: "Consumer",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TerminalID",
                table: "Consumer");

            migrationBuilder.CreateIndex(
                name: "IX_Consumer_MerchantID",
                table: "Consumer",
                column: "MerchantID");

            migrationBuilder.AddForeignKey(
                name: "FK_Consumer_Merchant_MerchantID",
                table: "Consumer",
                column: "MerchantID",
                principalTable: "Merchant",
                principalColumn: "MerchantID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
