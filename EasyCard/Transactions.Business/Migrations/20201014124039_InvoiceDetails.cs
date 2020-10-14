using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class InvoiceDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "InvoiceType",
                table: "Invoice",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<string>(
                name: "DefaultInvoiceItem",
                table: "Invoice",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "Invoice",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceSubject",
                table: "Invoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SendCCTo",
                table: "Invoice",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultInvoiceItem",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InvoiceSubject",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "SendCCTo",
                table: "Invoice");

            migrationBuilder.AlterColumn<short>(
                name: "InvoiceType",
                table: "Invoice",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);
        }
    }
}
