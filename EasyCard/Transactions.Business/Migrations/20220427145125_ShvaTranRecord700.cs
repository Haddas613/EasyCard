using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class ShvaTranRecord700 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShvaTranRecord",
                table: "PaymentTransaction",
                type: "varchar(700)",
                unicode: false,
                maxLength: 700,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(600)",
                oldUnicode: false,
                oldMaxLength: 600,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShvaTranRecord",
                table: "NayaxTransactionsParameters",
                type: "varchar(700)",
                unicode: false,
                maxLength: 700,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(600)",
                oldUnicode: false,
                oldMaxLength: 600,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShvaTranRecord",
                table: "PaymentTransaction",
                type: "varchar(600)",
                unicode: false,
                maxLength: 600,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(700)",
                oldUnicode: false,
                oldMaxLength: 700,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShvaTranRecord",
                table: "NayaxTransactionsParameters",
                type: "varchar(600)",
                unicode: false,
                maxLength: 600,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(700)",
                oldUnicode: false,
                oldMaxLength: 700,
                oldNullable: true);
        }
    }
}
