using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class PaymentRequestHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentRequestHistories",
                columns: table => new
                {
                    PaymentRequestHistoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentRequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OperationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperationDoneBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationDoneByID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OperationCode = table.Column<short>(type: "smallint", nullable: false),
                    OperationDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequestHistories", x => x.PaymentRequestHistoryID);
                    table.ForeignKey(
                        name: "FK_PaymentRequestHistories_PaymentRequest_PaymentRequestID",
                        column: x => x.PaymentRequestID,
                        principalTable: "PaymentRequest",
                        principalColumn: "PaymentRequestID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestHistories_PaymentRequestID",
                table: "PaymentRequestHistories",
                column: "PaymentRequestID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentRequestHistories");
        }
    }
}
