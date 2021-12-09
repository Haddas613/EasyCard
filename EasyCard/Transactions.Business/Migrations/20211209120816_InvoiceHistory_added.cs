using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class InvoiceHistory_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvoiceHistory",
                columns: table => new
                {
                    InvoiceHistoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OperationDoneBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationDoneByID = table.Column<Guid>(type: "uniqueidentifier", unicode: false, maxLength: 50, nullable: true),
                    OperationCode = table.Column<short>(type: "smallint", unicode: false, maxLength: 30, nullable: false),
                    OperationDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationMessage = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CorrelationId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SourceIP = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceHistory", x => x.InvoiceHistoryID);
                    table.ForeignKey(
                        name: "FK_InvoiceHistory_Invoice_InvoiceID",
                        column: x => x.InvoiceID,
                        principalTable: "Invoice",
                        principalColumn: "InvoiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHistory_InvoiceID",
                table: "InvoiceHistory",
                column: "InvoiceID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceHistory");
        }
    }
}
