using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class ExternalSystem_Removed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TerminalExternalSystem_ExternalSystem_ExternalSystemID",
                table: "TerminalExternalSystem");

            migrationBuilder.DropTable(
                name: "ExternalSystem");

            migrationBuilder.DropIndex(
                name: "IX_TerminalExternalSystem_ExternalSystemID",
                table: "TerminalExternalSystem");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TerminalExternalSystem",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "TerminalExternalSystem");

            migrationBuilder.CreateTable(
                name: "ExternalSystem",
                columns: table => new
                {
                    ExternalSystemID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstanceTypeFullName = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SettingsTypeFullName = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UpdateTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalSystem", x => x.ExternalSystemID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerminalExternalSystem_ExternalSystemID",
                table: "TerminalExternalSystem",
                column: "ExternalSystemID");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSystem_Name",
                table: "ExternalSystem",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TerminalExternalSystem_ExternalSystem_ExternalSystemID",
                table: "TerminalExternalSystem",
                column: "ExternalSystemID",
                principalTable: "ExternalSystem",
                principalColumn: "ExternalSystemID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
