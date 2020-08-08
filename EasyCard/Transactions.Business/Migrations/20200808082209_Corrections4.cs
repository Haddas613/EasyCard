using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class Corrections4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InitialTransactionID",
                table: "CreditCardTokenDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    InvoiceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TerminalID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoicingID = table.Column<long>(type: "bigint", nullable: true),
                    InvoiceType = table.Column<short>(type: "smallint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Currency = table.Column<short>(type: "smallint", nullable: false),
                    NumberOfPayments = table.Column<int>(type: "int", nullable: false),
                    InvoiceAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    CardOwnerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CardOwnerNationalID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    DealReference = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DealDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsumerEmail = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ConsumerPhone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    ConsumerID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Items = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    OperationDoneBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationDoneByID = table.Column<Guid>(type: "uniqueidentifier", unicode: false, maxLength: 50, nullable: true),
                    CorrelationId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SourceIP = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.InvoiceID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoice");

            migrationBuilder.DropColumn(
                name: "InitialTransactionID",
                table: "CreditCardTokenDetails");
        }
    }
}
