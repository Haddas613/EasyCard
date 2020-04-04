using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditCardTokenDetails",
                columns: table => new
                {
                    CreditCardTokenID = table.Column<Guid>(nullable: false),
                    CardNumber = table.Column<string>(unicode: false, maxLength: 20, nullable: false),
                    CardExpiration = table.Column<string>(unicode: false, maxLength: 5, nullable: true),
                    CardVendor = table.Column<string>(unicode: false, maxLength: 20, nullable: true),
                    CardOwnerName = table.Column<string>(maxLength: 50, nullable: true),
                    CardOwnerNationalID = table.Column<string>(unicode: false, maxLength: 20, nullable: true),
                    TerminalID = table.Column<Guid>(nullable: false),
                    MerchantID = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: true),
                    Active = table.Column<bool>(nullable: false),
                    OperationDoneBy = table.Column<string>(maxLength: 50, nullable: false),
                    OperationDoneByID = table.Column<Guid>(unicode: false, maxLength: 50, nullable: true),
                    CorrelationId = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    SourceIP = table.Column<string>(unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCardTokenDetails", x => x.CreditCardTokenID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransaction",
                columns: table => new
                {
                    PaymentTransactionID = table.Column<Guid>(nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "date", nullable: false),
                    TransactionTimestamp = table.Column<DateTime>(nullable: true),
                    InitialTransactionID = table.Column<Guid>(nullable: true),
                    TerminalID = table.Column<Guid>(nullable: false),
                    MerchantID = table.Column<Guid>(nullable: false),
                    ProcessorID = table.Column<long>(nullable: true),
                    AggregatorID = table.Column<long>(nullable: true),
                    InvoicingID = table.Column<long>(nullable: true),
                    MarketerID = table.Column<long>(nullable: true),
                    Status = table.Column<short>(nullable: false),
                    TransactionType = table.Column<short>(nullable: false),
                    RejectionReason = table.Column<short>(nullable: true),
                    Currency = table.Column<short>(nullable: false),
                    CardPresence = table.Column<short>(nullable: false),
                    NumberOfPayments = table.Column<int>(nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    InitialPaymentAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    InstallmentPaymentAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    CardNumber = table.Column<string>(unicode: false, maxLength: 20, nullable: true),
                    CardExpiration = table.Column<string>(unicode: false, maxLength: 5, nullable: true),
                    CardVendor = table.Column<string>(unicode: false, maxLength: 20, nullable: true),
                    CardOwnerName = table.Column<string>(unicode: false, maxLength: 100, nullable: true),
                    CardOwnerNationalID = table.Column<string>(unicode: false, maxLength: 20, nullable: true),
                    CardBin = table.Column<string>(unicode: false, maxLength: 10, nullable: true),
                    CreditCardToken = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    DealReference = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    DealDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsumerEmail = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    ConsumerPhone = table.Column<string>(unicode: false, maxLength: 20, nullable: true),
                    ShvaShovarNumber = table.Column<string>(unicode: false, maxLength: 20, nullable: true),
                    ShvaDealID = table.Column<string>(unicode: false, maxLength: 20, nullable: true),
                    ShvaTransmissionNumber = table.Column<string>(unicode: false, maxLength: 20, nullable: true),
                    ShvaTerminalID = table.Column<string>(unicode: false, maxLength: 20, nullable: true),
                    ShvaTransmissionDate = table.Column<DateTime>(nullable: true),
                    ManuallyTransmitted = table.Column<bool>(nullable: true),
                    ClearingHouseTransactionID = table.Column<long>(nullable: true),
                    ClearingHouseMerchantReference = table.Column<Guid>(unicode: false, maxLength: 50, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdateTimestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ConsumerIP = table.Column<string>(unicode: false, maxLength: 32, nullable: true),
                    MerchantIP = table.Column<string>(unicode: false, maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransaction", x => x.PaymentTransactionID);
                });

            migrationBuilder.CreateTable(
                name: "TransactionHistory",
                columns: table => new
                {
                    TransactionHistoryID = table.Column<Guid>(nullable: false),
                    PaymentTransactionID = table.Column<Guid>(nullable: false),
                    OperationDate = table.Column<DateTime>(nullable: false),
                    OperationDoneBy = table.Column<string>(maxLength: 50, nullable: false),
                    OperationDoneByID = table.Column<Guid>(unicode: false, maxLength: 50, nullable: true),
                    OperationCode = table.Column<short>(unicode: false, maxLength: 30, nullable: false),
                    OperationDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationMessage = table.Column<string>(maxLength: 250, nullable: true),
                    AdditionalDetails = table.Column<string>(nullable: true),
                    CorrelationId = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    IntegrationMessageId = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    SourceIP = table.Column<string>(unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionHistory", x => x.TransactionHistoryID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditCardTokenDetails");

            migrationBuilder.DropTable(
                name: "PaymentTransaction");

            migrationBuilder.DropTable(
                name: "TransactionHistory");
        }
    }
}
