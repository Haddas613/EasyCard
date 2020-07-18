using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class Billing6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "BillingDeal",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "BillingDeal",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperationDoneBy",
                table: "BillingDeal",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OperationDoneByID",
                table: "BillingDeal",
                type: "uniqueidentifier",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceIP",
                table: "BillingDeal",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "OperationDoneBy",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "OperationDoneByID",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "SourceIP",
                table: "BillingDeal");
        }
    }
}
