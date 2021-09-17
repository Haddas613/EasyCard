using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class CardExpirationDate_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<DateTime>(
            //    name: "CardExpirationDate",
            //    table: "vFutureBillings",
            //    type: "date",
            //    nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CardExpirationDate",
                table: "PaymentTransaction",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CardExpirationDate",
                table: "CreditCardTokenDetails",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CardExpirationDate",
                table: "BillingDeal",
                type: "date",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "CardExpirationDate",
            //    table: "vFutureBillings");

            migrationBuilder.DropColumn(
                name: "CardExpirationDate",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "CardExpirationDate",
                table: "CreditCardTokenDetails");

            migrationBuilder.DropColumn(
                name: "CardExpirationDate",
                table: "BillingDeal");
        }
    }
}
