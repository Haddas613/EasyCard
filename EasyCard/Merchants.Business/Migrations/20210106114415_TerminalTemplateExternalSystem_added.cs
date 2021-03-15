using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class TerminalTemplateExternalSystem_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TerminalTemplateExternalSystem",
                columns: table => new
                {
                    TerminalTemplateExternalSystemID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalSystemID = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TerminalTemplateID = table.Column<long>(type: "bigint", nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminalTemplateExternalSystem", x => x.TerminalTemplateExternalSystemID);
                    table.ForeignKey(
                        name: "FK_TerminalTemplateExternalSystem_TerminalTemplate_TerminalTemplateID",
                        column: x => x.TerminalTemplateID,
                        principalTable: "TerminalTemplate",
                        principalColumn: "TerminalTemplateID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerminalTemplateExternalSystem_TerminalTemplateID",
                table: "TerminalTemplateExternalSystem",
                column: "TerminalTemplateID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerminalTemplateExternalSystem");
        }
    }
}
