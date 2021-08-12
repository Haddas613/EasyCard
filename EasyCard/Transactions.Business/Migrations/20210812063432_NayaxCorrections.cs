using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class NayaxCorrections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NayaxTransactionsParameters",
                columns: table => new
                {
                    NayaxTransactionsParametersID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NayaxTransactionsParametersTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PinPadTransactionID = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ShvaTranRecord = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NayaxTransactionsParameters", x => x.NayaxTransactionsParametersID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NayaxTransactionsParameters_PinPadTransactionID",
                table: "NayaxTransactionsParameters",
                column: "PinPadTransactionID",
                unique: true,
                filter: "[PinPadTransactionID] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NayaxTransactionsParameters");
        }
    }
}
