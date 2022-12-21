using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class AdditionalShvaFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthSolekNum",
                table: "PaymentTransaction",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShvaTransactionDate",
                table: "PaymentTransaction",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthSolekNum",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ShvaTransactionDate",
                table: "PaymentTransaction");
        }
    }
}
