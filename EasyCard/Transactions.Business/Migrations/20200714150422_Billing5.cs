using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class Billing5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CreditCardToken",
                table: "PaymentTransaction",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "BillingDeal",
                columns: table => new
                {
                    BillingDealID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingDealTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InitialTransactionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TerminalID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessorID = table.Column<long>(type: "bigint", nullable: true),
                    AggregatorID = table.Column<long>(type: "bigint", nullable: true),
                    InvoicingID = table.Column<long>(type: "bigint", nullable: true),
                    MarketerID = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Currency = table.Column<short>(type: "smallint", nullable: false),
                    NumberOfPayments = table.Column<int>(type: "int", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    CurrentDeal = table.Column<int>(type: "int", nullable: true),
                    CardNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    CardExpiration = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
                    CardVendor = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    CardOwnerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CardOwnerNationalID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    CardBin = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    CreditCardToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DealReference = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DealDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsumerEmail = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ConsumerPhone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    ConsumerID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Items = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillingSchedule = table.Column<string>(type: "nvarchar(max)", unicode: false, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingDeal", x => x.BillingDealID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingDeal");

            migrationBuilder.AlterColumn<string>(
                name: "CreditCardToken",
                table: "PaymentTransaction",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
