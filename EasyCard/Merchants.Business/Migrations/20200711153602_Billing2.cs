using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class Billing2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantHistory_Merchant_MerchantID",
                table: "MerchantHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminalExternalSystem_Terminal_TerminalID",
                table: "TerminalExternalSystem");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTerminalMapping_Terminal_TerminalID",
                table: "UserTerminalMapping");

            migrationBuilder.DropColumn(
                name: "Users",
                table: "Merchant");

            migrationBuilder.AlterColumn<Guid>(
                name: "MerchantID",
                table: "Terminal",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Consumer",
                columns: table => new
                {
                    ConsumerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    UpdateTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    ConsumerEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConsumerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConsumerPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConsumerAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperationDoneBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationDoneByID = table.Column<Guid>(type: "uniqueidentifier", unicode: false, maxLength: 50, nullable: true),
                    CorrelationId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SourceIP = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumer", x => x.ConsumerID);
                    table.ForeignKey(
                        name: "FK_Consumer_Merchant_MerchantID",
                        column: x => x.MerchantID,
                        principalTable: "Merchant",
                        principalColumn: "MerchantID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    UpdateTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(19,4)", nullable: false),
                    Currency = table.Column<short>(type: "smallint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperationDoneBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationDoneByID = table.Column<Guid>(type: "uniqueidentifier", unicode: false, maxLength: 50, nullable: true),
                    CorrelationId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SourceIP = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_Item_Merchant_MerchantID",
                        column: x => x.MerchantID,
                        principalTable: "Merchant",
                        principalColumn: "MerchantID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consumer_MerchantID",
                table: "Consumer",
                column: "MerchantID");

            migrationBuilder.CreateIndex(
                name: "IX_Item_MerchantID",
                table: "Item",
                column: "MerchantID");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantHistory_Merchant_MerchantID",
                table: "MerchantHistory",
                column: "MerchantID",
                principalTable: "Merchant",
                principalColumn: "MerchantID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TerminalExternalSystem_Terminal_TerminalID",
                table: "TerminalExternalSystem",
                column: "TerminalID",
                principalTable: "Terminal",
                principalColumn: "TerminalID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTerminalMapping_Terminal_TerminalID",
                table: "UserTerminalMapping",
                column: "TerminalID",
                principalTable: "Terminal",
                principalColumn: "TerminalID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantHistory_Merchant_MerchantID",
                table: "MerchantHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminalExternalSystem_Terminal_TerminalID",
                table: "TerminalExternalSystem");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTerminalMapping_Terminal_TerminalID",
                table: "UserTerminalMapping");

            migrationBuilder.DropTable(
                name: "Consumer");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.AlterColumn<Guid>(
                name: "MerchantID",
                table: "Terminal",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Users",
                table: "Merchant",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantHistory_Merchant_MerchantID",
                table: "MerchantHistory",
                column: "MerchantID",
                principalTable: "Merchant",
                principalColumn: "MerchantID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TerminalExternalSystem_Terminal_TerminalID",
                table: "TerminalExternalSystem",
                column: "TerminalID",
                principalTable: "Terminal",
                principalColumn: "TerminalID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTerminalMapping_Terminal_TerminalID",
                table: "UserTerminalMapping",
                column: "TerminalID",
                principalTable: "Terminal",
                principalColumn: "TerminalID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
