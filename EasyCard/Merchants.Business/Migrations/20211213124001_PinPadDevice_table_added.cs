using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class PinPadDevice_table_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PinPadDevice",
                columns: table => new
                {
                    PinPadDeviceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceTerminalID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PosName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TerminalID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinPadDevice", x => x.PinPadDeviceID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PinPadDevice");
        }
    }
}
