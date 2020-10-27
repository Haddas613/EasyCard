using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class PaymentRequestHistory2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestHistories_PaymentRequest_PaymentRequestID",
                table: "PaymentRequestHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentRequestHistories",
                table: "PaymentRequestHistories");

            migrationBuilder.RenameTable(
                name: "PaymentRequestHistories",
                newName: "PaymentRequestHistory");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestHistories_PaymentRequestID",
                table: "PaymentRequestHistory",
                newName: "IX_PaymentRequestHistory_PaymentRequestID");

            migrationBuilder.AlterColumn<string>(
                name: "SourceIP",
                table: "PaymentRequestHistory",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentRequestID",
                table: "PaymentRequestHistory",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OperationMessage",
                table: "PaymentRequestHistory",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OperationDoneBy",
                table: "PaymentRequestHistory",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OperationDate",
                table: "PaymentRequestHistory",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "PaymentRequestHistory",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentRequestHistory",
                table: "PaymentRequestHistory",
                column: "PaymentRequestHistoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestHistory_PaymentRequest_PaymentRequestID",
                table: "PaymentRequestHistory",
                column: "PaymentRequestID",
                principalTable: "PaymentRequest",
                principalColumn: "PaymentRequestID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestHistory_PaymentRequest_PaymentRequestID",
                table: "PaymentRequestHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentRequestHistory",
                table: "PaymentRequestHistory");

            migrationBuilder.RenameTable(
                name: "PaymentRequestHistory",
                newName: "PaymentRequestHistories");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestHistory_PaymentRequestID",
                table: "PaymentRequestHistories",
                newName: "IX_PaymentRequestHistories_PaymentRequestID");

            migrationBuilder.AlterColumn<string>(
                name: "SourceIP",
                table: "PaymentRequestHistories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentRequestID",
                table: "PaymentRequestHistories",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "OperationMessage",
                table: "PaymentRequestHistories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OperationDoneBy",
                table: "PaymentRequestHistories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OperationDate",
                table: "PaymentRequestHistories",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "PaymentRequestHistories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentRequestHistories",
                table: "PaymentRequestHistories",
                column: "PaymentRequestHistoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestHistories_PaymentRequest_PaymentRequestID",
                table: "PaymentRequestHistories",
                column: "PaymentRequestID",
                principalTable: "PaymentRequest",
                principalColumn: "PaymentRequestID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
