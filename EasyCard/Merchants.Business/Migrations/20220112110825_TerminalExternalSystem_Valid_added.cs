using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class TerminalExternalSystem_Valid_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consumer_TerminalID",
                table: "Consumer");

            migrationBuilder.DropIndex(
                name: "IX_Consumer_TerminalID_ConsumerID",
                table: "Consumer");

            migrationBuilder.DropIndex(
                name: "IX_Consumer_TerminalID_ExternalReference",
                table: "Consumer");

            migrationBuilder.AddColumn<bool>(
                name: "Valid",
                table: "TerminalExternalSystem",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Valid",
                table: "TerminalExternalSystem");

            migrationBuilder.CreateIndex(
                name: "IX_Consumer_TerminalID",
                table: "Consumer",
                column: "TerminalID");

            migrationBuilder.CreateIndex(
                name: "IX_Consumer_TerminalID_ConsumerID",
                table: "Consumer",
                columns: new[] { "TerminalID", "ConsumerID" });

            migrationBuilder.CreateIndex(
                name: "IX_Consumer_TerminalID_ExternalReference",
                table: "Consumer",
                columns: new[] { "TerminalID", "ExternalReference" });
        }
    }
}
