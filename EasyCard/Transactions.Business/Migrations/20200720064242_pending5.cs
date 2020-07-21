using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class pending5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BillingDealID",
                table: "PaymentTransaction",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentTransactionID",
                table: "BillingDeal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentTransactionTimestamp",
                table: "BillingDeal",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingDealID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "CurrentTransactionID",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "CurrentTransactionTimestamp",
                table: "BillingDeal");
        }
    }
}
