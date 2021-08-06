using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class Billing_and_Transaction_BankDetails_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bank",
                table: "PaymentTransaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "PaymentTransaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BankBranch",
                table: "PaymentTransaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "BankTransferDetails_PaymentType",
                table: "PaymentTransaction",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "PaymentTransaction",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "PaymentTransaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bank",
                table: "BillingDeal",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "BillingDeal",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BankBranch",
                table: "BillingDeal",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "BankDetails_PaymentType",
                table: "BillingDeal",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "PaymentType",
                table: "BillingDeal",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bank",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "BankBranch",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "BankTransferDetails_PaymentType",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "Bank",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "BankBranch",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "BankDetails_PaymentType",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "BillingDeal");
        }
    }
}
