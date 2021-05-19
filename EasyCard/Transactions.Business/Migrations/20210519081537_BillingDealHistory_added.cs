using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class BillingDealHistory_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillingDealHistory",
                columns: table => new
                {
                    BillingDealHistoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingDealID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_BillingDealHistory", x => x.BillingDealHistoryID);
                    table.ForeignKey(
                        name: "FK_BillingDealHistory_BillingDeal_BillingDealID",
                        column: x => x.BillingDealID,
                        principalTable: "BillingDeal",
                        principalColumn: "BillingDealID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingDealHistory_BillingDealID",
                table: "BillingDealHistory",
                column: "BillingDealID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingDealHistory");
        }
    }
}
