using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class CurrencyRates2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CurrencyRates",
                table: "CurrencyRates");

            migrationBuilder.RenameTable(
                name: "CurrencyRates",
                newName: "CurrencyRate");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "CurrencyRate",
                type: "decimal(19,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "CurrencyRate",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CurrencyRate",
                table: "CurrencyRate",
                column: "CurrencyRateID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CurrencyRate",
                table: "CurrencyRate");

            migrationBuilder.RenameTable(
                name: "CurrencyRate",
                newName: "CurrencyRates");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "CurrencyRates",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "CurrencyRates",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CurrencyRates",
                table: "CurrencyRates",
                column: "CurrencyRateID");
        }
    }
}
