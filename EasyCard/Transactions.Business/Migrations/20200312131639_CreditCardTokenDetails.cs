using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class CreditCardTokenDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditCardTokenDetails",
                columns: table => new
                {
                    CreditCardTokenID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicKey = table.Column<string>(unicode: false, maxLength: 64, nullable: false),
                    Hash = table.Column<string>(unicode: false, maxLength: 256, nullable: false),
                    TerminalID = table.Column<long>(nullable: false),
                    MerchantID = table.Column<long>(nullable: false),
                    CardNumber = table.Column<string>(unicode: false, maxLength: 16, nullable: false),
                    CardExpiration = table.Column<string>(unicode: false, maxLength: 5, nullable: true),
                    CardVendor = table.Column<string>(nullable: false),
                    CardOwnerNationalID = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCardTokenDetails", x => x.CreditCardTokenID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransaction",
                columns: table => new
                {
                    PaymentTransactionID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialTransactionID = table.Column<long>(nullable: true),
                    TransactionNumber = table.Column<long>(nullable: false),
                    TerminalID = table.Column<long>(nullable: false),
                    MerchantID = table.Column<long>(nullable: false),
                    ProcessorID = table.Column<long>(nullable: true),
                    AggregatorID = table.Column<long>(nullable: true),
                    InvoicingID = table.Column<long>(nullable: true),
                    MarketerID = table.Column<long>(nullable: true),
                    Status = table.Column<short>(nullable: false),
                    TransactionType = table.Column<short>(nullable: false),
                    RejectionReason = table.Column<int>(nullable: true),
                    Currency = table.Column<int>(nullable: false),
                    CardPresence = table.Column<int>(nullable: false),
                    NumberOfInstallments = table.Column<int>(nullable: false),
                    CurrentInstallment = table.Column<int>(nullable: false),
                    TransactionAmount = table.Column<decimal>(nullable: false),
                    InitialPaymentAmount = table.Column<decimal>(nullable: false),
                    TotalAmount = table.Column<decimal>(nullable: false),
                    InstallmentPaymentAmount = table.Column<decimal>(nullable: false),
                    TransactionDate = table.Column<DateTime>(nullable: true),
                    TransactionTimestamp = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    BillingOrderID = table.Column<long>(nullable: true),
                    UpdateTimestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreditCardDetails_CardNumber = table.Column<string>(nullable: true),
                    CardExpiration = table.Column<string>(unicode: false, maxLength: 5, nullable: true),
                    CreditCardDetails_CardBin = table.Column<string>(nullable: true),
                    CreditCardDetails_CardVendor = table.Column<string>(nullable: true),
                    CreditCardDetails_CardOwnerNationalId = table.Column<string>(nullable: true),
                    CreditCardDetails_CardOwnerName = table.Column<string>(nullable: true),
                    CreditCardDetails_CardToken = table.Column<string>(nullable: true),
                    DealDescription = table.Column<string>(nullable: true),
                    ConsumerEmail = table.Column<string>(nullable: true),
                    ConsumerPhone = table.Column<string>(nullable: true),
                    ConsumerIP = table.Column<string>(nullable: true),
                    MerchantIP = table.Column<string>(nullable: true),
                    ShvaShovarNumber = table.Column<string>(nullable: true),
                    ShvaDealID = table.Column<string>(nullable: true),
                    ShvaTransmissionNumber = table.Column<string>(nullable: true),
                    ShvaTerminalID = table.Column<string>(nullable: true),
                    ShvaTransactionDetails_TransmissionDate = table.Column<DateTime>(nullable: true),
                    ManuallyTransmitted = table.Column<bool>(nullable: true),
                    ClearingHouseTransactionID = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransaction", x => x.PaymentTransactionID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditCardTokenDetails");

            migrationBuilder.DropTable(
                name: "PaymentTransaction");
        }
    }
}
