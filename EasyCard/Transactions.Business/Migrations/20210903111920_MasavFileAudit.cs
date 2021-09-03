using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class MasavFileAudit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "MasavFile",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperationDoneBy",
                table: "MasavFile",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OperationDoneByID",
                table: "MasavFile",
                type: "uniqueidentifier",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceIP",
                table: "MasavFile",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "MasavFile");

            migrationBuilder.DropColumn(
                name: "OperationDoneBy",
                table: "MasavFile");

            migrationBuilder.DropColumn(
                name: "OperationDoneByID",
                table: "MasavFile");

            migrationBuilder.DropColumn(
                name: "SourceIP",
                table: "MasavFile");
        }
    }
}
