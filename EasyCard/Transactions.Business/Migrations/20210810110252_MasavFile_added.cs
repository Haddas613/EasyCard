using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class MasavFile_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MasavFile",
                columns: table => new
                {
                    MasavFileID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasavFileDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransactionAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: true),
                    StorageReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstituteNumber = table.Column<int>(type: "int", nullable: true),
                    SendingInstitute = table.Column<int>(type: "int", nullable: true),
                    InstituteName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Currency = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasavFile", x => x.MasavFileID);
                });

            migrationBuilder.CreateTable(
                name: "MasavFileRow",
                columns: table => new
                {
                    MasavFileRowID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasavFileID = table.Column<long>(type: "bigint", nullable: true),
                    PaymentTransactionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TerminalID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Bankcode = table.Column<int>(type: "int", nullable: true),
                    BranchNumber = table.Column<int>(type: "int", nullable: true),
                    AccountNumber = table.Column<int>(type: "int", nullable: true),
                    NationalID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(19,4)", nullable: true),
                    IsPayed = table.Column<bool>(type: "bit", nullable: true),
                    SmsSent = table.Column<bool>(type: "bit", nullable: false),
                    ComissionTotal = table.Column<decimal>(type: "decimal(19,4)", nullable: true),
                    SmsSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasavFileRow", x => x.MasavFileRowID);
                    table.ForeignKey(
                        name: "FK_MasavFileRow_MasavFile_MasavFileID",
                        column: x => x.MasavFileID,
                        principalTable: "MasavFile",
                        principalColumn: "MasavFileID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MasavFileRow_MasavFileID",
                table: "MasavFileRow",
                column: "MasavFileID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasavFileRow");

            migrationBuilder.DropTable(
                name: "MasavFile");
        }
    }
}
