using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class TerminalTemplates_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TerminalTemplateID",
                table: "TerminalExternalSystem",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TerminalTemplateID",
                table: "Feature",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TerminalTemplate",
                columns: table => new
                {
                    TerminalTemplateID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdateTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Settings = table.Column<string>(type: "nvarchar(max)", unicode: false, nullable: true),
                    BillingSettings = table.Column<string>(type: "nvarchar(max)", unicode: false, nullable: true),
                    InvoiceSettings = table.Column<string>(type: "nvarchar(max)", unicode: false, nullable: true),
                    PaymentRequestSettings = table.Column<string>(type: "nvarchar(max)", unicode: false, nullable: true),
                    CheckoutSettings = table.Column<string>(type: "nvarchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminalTemplate", x => x.TerminalTemplateID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerminalExternalSystem_TerminalTemplateID",
                table: "TerminalExternalSystem",
                column: "TerminalTemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_Feature_TerminalTemplateID",
                table: "Feature",
                column: "TerminalTemplateID");

            migrationBuilder.AddForeignKey(
                name: "FK_Feature_TerminalTemplate_TerminalTemplateID",
                table: "Feature",
                column: "TerminalTemplateID",
                principalTable: "TerminalTemplate",
                principalColumn: "TerminalTemplateID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TerminalExternalSystem_TerminalTemplate_TerminalTemplateID",
                table: "TerminalExternalSystem",
                column: "TerminalTemplateID",
                principalTable: "TerminalTemplate",
                principalColumn: "TerminalTemplateID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feature_TerminalTemplate_TerminalTemplateID",
                table: "Feature");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminalExternalSystem_TerminalTemplate_TerminalTemplateID",
                table: "TerminalExternalSystem");

            migrationBuilder.DropTable(
                name: "TerminalTemplate");

            migrationBuilder.DropIndex(
                name: "IX_TerminalExternalSystem_TerminalTemplateID",
                table: "TerminalExternalSystem");

            migrationBuilder.DropIndex(
                name: "IX_Feature_TerminalTemplateID",
                table: "Feature");

            migrationBuilder.DropColumn(
                name: "TerminalTemplateID",
                table: "TerminalExternalSystem");

            migrationBuilder.DropColumn(
                name: "TerminalTemplateID",
                table: "Feature");
        }
    }
}
