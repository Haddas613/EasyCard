using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class RemoveFutureBillingsView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "vFutureBillings");

            migrationBuilder.Sql("DROP VIEW vFutureBillings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vFutureBillings",
                columns: table => new
                {
                    BillingDealID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentDeal = table.Column<int>(type: "int", nullable: false),
                    BillingDealTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CardExpiration = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
                    FutureBilling_CardNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FutureBilling_CardOwnerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<short>(type: "smallint", nullable: false),
                    FutureDeal = table.Column<int>(type: "int", nullable: true),
                    FutureScheduledTransaction = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MerchantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NextScheduledTransaction = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PausedFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PausedTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TerminalID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    CardExpirationDate = table.Column<DateTime>(type: "date", nullable: true),
                    CardNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    CardOwnerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreditCardDetails_ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vFutureBillings", x => new { x.BillingDealID, x.CurrentDeal });
                });
        }
    }
}
