using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentityServer.Data.Migrations
{
    public partial class ApplicationUser_PasswordUpdated_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerminalApiAuthKey");

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordUpdated",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordUpdated",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "TerminalApiAuthKey",
                columns: table => new
                {
                    TerminalApiAuthKeyID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthKey = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TerminalID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminalApiAuthKey", x => x.TerminalApiAuthKeyID);
                });
        }
    }
}
