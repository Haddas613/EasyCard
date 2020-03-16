using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class MerchantHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MerchantHistory",
                columns: table => new
                {
                    MerchantHistoryID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantID = table.Column<long>(nullable: false),
                    OperationDate = table.Column<DateTime>(nullable: false),
                    OperationCode = table.Column<string>(unicode: false, maxLength: 30, nullable: false),
                    OperationDoneBy = table.Column<string>(maxLength: 50, nullable: false),
                    OperationDoneByID = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    OperationDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    SourceIP = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    ReasonForChange = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantHistory", x => x.MerchantHistoryID);
                    table.ForeignKey(
                        name: "FK_MerchantHistory_Merchant_MerchantID",
                        column: x => x.MerchantID,
                        principalTable: "Merchant",
                        principalColumn: "MerchantID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantHistory_MerchantID",
                table: "MerchantHistory",
                column: "MerchantID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MerchantHistory");
        }
    }
}
