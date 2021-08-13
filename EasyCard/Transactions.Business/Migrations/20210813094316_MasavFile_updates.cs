using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class MasavFile_updates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComissionTotal",
                table: "MasavFileRow");

            migrationBuilder.DropColumn(
                name: "PayedDate",
                table: "MasavFileRow");

            migrationBuilder.RenameColumn(
                name: "TerminalID",
                table: "MasavFileRow",
                newName: "ConsumerID");

            migrationBuilder.AddColumn<string>(
                name: "ConsumerName",
                table: "MasavFileRow",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TerminalID",
                table: "MasavFile",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumerName",
                table: "MasavFileRow");

            migrationBuilder.DropColumn(
                name: "TerminalID",
                table: "MasavFile");

            migrationBuilder.RenameColumn(
                name: "ConsumerID",
                table: "MasavFileRow",
                newName: "TerminalID");

            migrationBuilder.AddColumn<decimal>(
                name: "ComissionTotal",
                table: "MasavFileRow",
                type: "decimal(19,4)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PayedDate",
                table: "MasavFileRow",
                type: "datetime2",
                nullable: true);
        }
    }
}
