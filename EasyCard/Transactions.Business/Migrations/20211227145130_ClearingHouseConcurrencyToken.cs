using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class ClearingHouseConcurrencyToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "PaymentTransaction",
                newName: "ClearingHouseConcurrencyToken");

            migrationBuilder.AlterColumn<string>(
                name: "ClearingHouseConcurrencyToken",
                table: "PaymentTransaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClearingHouseConcurrencyToken",
                table: "PaymentTransaction",
                newName: "ConcurrencyToken");

            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyToken",
                table: "PaymentTransaction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
