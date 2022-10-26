using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class CardExpirationBeforeExtended : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CardExpirationBeforeExtendedDate",
                table: "CreditCardTokenDetails",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Extended",
                table: "CreditCardTokenDetails",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardExpirationBeforeExtendedDate",
                table: "CreditCardTokenDetails");

            migrationBuilder.DropColumn(
                name: "Extended",
                table: "CreditCardTokenDetails");
        }
    }
}
