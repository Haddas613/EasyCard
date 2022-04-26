using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class pinpadTranrecordReceiptnumber_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PinPadTranRecordReceiptNumber",
                table: "NayaxTransactionsParameters",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NayaxTransactionsParameters_PinPadTranRecordReceiptNumber",
                table: "NayaxTransactionsParameters",
                column: "PinPadTranRecordReceiptNumber",
                unique: true,
                filter: "[PinPadTranRecordReceiptNumber] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NayaxTransactionsParameters_PinPadTranRecordReceiptNumber",
                table: "NayaxTransactionsParameters");

            migrationBuilder.DropColumn(
                name: "PinPadTranRecordReceiptNumber",
                table: "NayaxTransactionsParameters");
        }
    }
}
