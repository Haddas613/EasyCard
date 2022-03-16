using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class TransactionAuditInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OperationDoneBy",
                table: "PaymentTransaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationDoneByID",
                table: "PaymentTransaction",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperationDoneBy",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "OperationDoneByID",
                table: "PaymentTransaction");
        }
    }
}
