using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class Consumer_ConsumerPhone_Nullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ConsumerPhone",
                table: "Consumer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "ConsumerPhone",
                table: "Consumer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
