using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class TokenUpdateDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TokenCreated",
                table: "BillingDeal",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenUpdated",
                table: "BillingDeal",
                type: "date",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenCreated",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "TokenUpdated",
                table: "BillingDeal");
        }
    }
}
