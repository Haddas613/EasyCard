using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class BillingDeal_PausedFrom_PausedTo_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PausedFrom",
                table: "BillingDeal",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PausedTo",
                table: "BillingDeal",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PausedFrom",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "PausedTo",
                table: "BillingDeal");
        }
    }
}
