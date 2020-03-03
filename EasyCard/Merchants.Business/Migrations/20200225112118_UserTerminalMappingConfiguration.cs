using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class UserTerminalMappingConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTerminalMappings");

            migrationBuilder.DropColumn(
                name: "Users",
                table: "Terminal");

            migrationBuilder.CreateTable(
                name: "UserTerminalMapping",
                columns: table => new
                {
                    UserTerminalMappingID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    TerminalID = table.Column<long>(nullable: false),
                    OperationDate = table.Column<DateTime>(nullable: false),
                    OperationDoneBy = table.Column<string>(maxLength: 50, nullable: true),
                    OperationDoneByID = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTerminalMapping", x => x.UserTerminalMappingID);
                    table.ForeignKey(
                        name: "FK_UserTerminalMapping_Terminal_TerminalID",
                        column: x => x.TerminalID,
                        principalTable: "Terminal",
                        principalColumn: "TerminalID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTerminalMapping_TerminalID",
                table: "UserTerminalMapping",
                column: "TerminalID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTerminalMapping_UserID_TerminalID",
                table: "UserTerminalMapping",
                columns: new[] { "UserID", "TerminalID" },
                unique: true,
                filter: "[UserID] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTerminalMapping");

            migrationBuilder.AddColumn<string>(
                name: "Users",
                table: "Terminal",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserTerminalMappings",
                columns: table => new
                {
                    UserTerminalMappingID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OperationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OperationDoneBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationDoneByID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TerminalID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTerminalMappings", x => x.UserTerminalMappingID);
                    table.ForeignKey(
                        name: "FK_UserTerminalMappings_Terminal_TerminalID",
                        column: x => x.TerminalID,
                        principalTable: "Terminal",
                        principalColumn: "TerminalID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTerminalMappings_TerminalID",
                table: "UserTerminalMappings",
                column: "TerminalID");
        }
    }
}
