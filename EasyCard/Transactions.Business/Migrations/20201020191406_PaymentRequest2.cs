using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class PaymentRequest2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Invoice");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "PaymentRequest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentTransactionID",
                table: "PaymentRequest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentTransactionID",
                table: "Invoice",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "PaymentTransactionID",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "PaymentTransactionID",
                table: "Invoice");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "PaymentRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Invoice",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
