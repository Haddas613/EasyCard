using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class RemoveInvoiceNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "BillingDeal");

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceID",
                table: "PaymentTransaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextScheduledTransaction",
                table: "BillingDeal",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "NextScheduledTransaction",
                table: "BillingDeal");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "PaymentRequest",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "BillingDeal",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
