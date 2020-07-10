using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class ShvaInitialDealDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthNum",
                table: "CreditCardTokenDetails",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthSolekNum",
                table: "CreditCardTokenDetails",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShvaDealID",
                table: "CreditCardTokenDetails",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShvaTransactionDate",
                table: "CreditCardTokenDetails",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthNum",
                table: "CreditCardTokenDetails");

            migrationBuilder.DropColumn(
                name: "AuthSolekNum",
                table: "CreditCardTokenDetails");

            migrationBuilder.DropColumn(
                name: "ShvaDealID",
                table: "CreditCardTokenDetails");

            migrationBuilder.DropColumn(
                name: "ShvaTransactionDate",
                table: "CreditCardTokenDetails");
        }
    }
}
