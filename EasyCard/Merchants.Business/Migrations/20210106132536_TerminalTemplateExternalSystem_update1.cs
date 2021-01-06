using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class TerminalTemplateExternalSystem_update1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TerminalExternalSystem_TerminalTemplate_TerminalTemplateID",
                table: "TerminalExternalSystem");

            migrationBuilder.DropIndex(
                name: "IX_TerminalExternalSystem_TerminalTemplateID",
                table: "TerminalExternalSystem");

            migrationBuilder.DropColumn(
                name: "TerminalTemplateID",
                table: "TerminalExternalSystem");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TerminalTemplateID",
                table: "TerminalExternalSystem",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TerminalExternalSystem_TerminalTemplateID",
                table: "TerminalExternalSystem",
                column: "TerminalTemplateID");

            migrationBuilder.AddForeignKey(
                name: "FK_TerminalExternalSystem_TerminalTemplate_TerminalTemplateID",
                table: "TerminalExternalSystem",
                column: "TerminalTemplateID",
                principalTable: "TerminalTemplate",
                principalColumn: "TerminalTemplateID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
