using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class Billing1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConsumerID",
                table: "PaymentTransaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Items",
                table: "PaymentTransaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerEmail",
                table: "CreditCardTokenDetails",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConsumerID",
                table: "CreditCardTokenDetails",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumerID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "Items",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ConsumerEmail",
                table: "CreditCardTokenDetails");

            migrationBuilder.DropColumn(
                name: "ConsumerID",
                table: "CreditCardTokenDetails");
        }
    }
}
