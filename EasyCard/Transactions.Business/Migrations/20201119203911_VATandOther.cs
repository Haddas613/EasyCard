using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class VATandOther : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultInvoiceItem",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "DefaultInvoiceItem",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "AggregatorID",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "InvoicingID",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "MarketerID",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "NumberOfPayments",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "ProcessorID",
                table: "BillingDeal");

            migrationBuilder.AddColumn<decimal>(
                name: "NetTotal",
                table: "PaymentTransaction",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VATRate",
                table: "PaymentTransaction",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VATTotal",
                table: "PaymentTransaction",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceSubject",
                table: "PaymentRequest",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromAddress",
                table: "PaymentRequest",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetTotal",
                table: "PaymentRequest",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RequestSubject",
                table: "PaymentRequest",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VATRate",
                table: "PaymentRequest",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VATTotal",
                table: "PaymentRequest",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceSubject",
                table: "Invoice",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetTotal",
                table: "Invoice",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VATRate",
                table: "Invoice",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VATTotal",
                table: "Invoice",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "BillingDeal",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceSubject",
                table: "BillingDeal",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "InvoiceType",
                table: "BillingDeal",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetTotal",
                table: "BillingDeal",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SendCCTo",
                table: "BillingDeal",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VATRate",
                table: "BillingDeal",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VATTotal",
                table: "BillingDeal",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetTotal",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "VATRate",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "VATTotal",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "FromAddress",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "NetTotal",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "RequestSubject",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "VATRate",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "VATTotal",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "NetTotal",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "VATRate",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "VATTotal",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "InvoiceSubject",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "InvoiceType",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "NetTotal",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "SendCCTo",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "VATRate",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "VATTotal",
                table: "BillingDeal");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceSubject",
                table: "PaymentRequest",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultInvoiceItem",
                table: "PaymentRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceSubject",
                table: "Invoice",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultInvoiceItem",
                table: "Invoice",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AggregatorID",
                table: "BillingDeal",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "InvoicingID",
                table: "BillingDeal",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MarketerID",
                table: "BillingDeal",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPayments",
                table: "BillingDeal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ProcessorID",
                table: "BillingDeal",
                type: "bigint",
                nullable: true);
        }
    }
}
