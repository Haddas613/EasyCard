using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class MasavFileCorrections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransactionAmount",
                table: "MasavFile",
                newName: "TotalAmount");

            migrationBuilder.AddColumn<long>(
                name: "MasavFileID",
                table: "PaymentTransaction",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NationalID",
                table: "MasavFileRow",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "MasavFileDate",
                table: "MasavFile",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MasavFileTimestamp",
                table: "MasavFile",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransaction_MerchantID_TerminalID",
                table: "PaymentTransaction",
                columns: new[] { "MerchantID", "TerminalID" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransaction_TerminalID_PaymentTypeEnum_MasavFileID",
                table: "PaymentTransaction",
                columns: new[] { "TerminalID", "PaymentTypeEnum", "MasavFileID" });

            migrationBuilder.CreateIndex(
                name: "IX_MasavFile_MasavFileDate",
                table: "MasavFile",
                column: "MasavFileDate");

            migrationBuilder.CreateIndex(
                name: "IX_MasavFile_TerminalID",
                table: "MasavFile",
                column: "TerminalID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransaction_MerchantID_TerminalID",
                table: "PaymentTransaction");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransaction_TerminalID_PaymentTypeEnum_MasavFileID",
                table: "PaymentTransaction");

            migrationBuilder.DropIndex(
                name: "IX_MasavFile_MasavFileDate",
                table: "MasavFile");

            migrationBuilder.DropIndex(
                name: "IX_MasavFile_TerminalID",
                table: "MasavFile");

            migrationBuilder.DropColumn(
                name: "MasavFileID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "MasavFileTimestamp",
                table: "MasavFile");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "MasavFile",
                newName: "TransactionAmount");

            migrationBuilder.AlterColumn<string>(
                name: "NationalID",
                table: "MasavFileRow",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "MasavFileDate",
                table: "MasavFile",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);
        }
    }
}
