using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class MiscTechDebt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTerminalMapping_UserID_TerminalID",
                table: "UserTerminalMapping");

            migrationBuilder.DropColumn(
                name: "ExternalProcessorReference",
                table: "TerminalExternalSystem");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantID",
                table: "UserTerminalMapping",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MerchantID",
                table: "UserTerminalMapping");

            migrationBuilder.AddColumn<string>(
                name: "ExternalProcessorReference",
                table: "TerminalExternalSystem",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTerminalMapping_UserID_TerminalID",
                table: "UserTerminalMapping",
                columns: new[] { "UserID", "TerminalID" },
                unique: true);
        }
    }
}
