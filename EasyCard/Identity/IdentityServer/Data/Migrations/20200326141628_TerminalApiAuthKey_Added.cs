using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentityServer.Data.Migrations
{
    public partial class TerminalApiAuthKey_Added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TerminalApiAuthKey",
                columns: table => new
                {
                    TerminalApiAuthKeyID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TerminalID = table.Column<long>(nullable: false),
                    AuthKey = table.Column<string>(unicode: false, maxLength: 512, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminalApiAuthKey", x => x.TerminalApiAuthKeyID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerminalApiAuthKey");
        }
    }
}
