using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class updateTranRecordtoMax : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShvaTranRecord",
                table: "PaymentTransaction",
                type: "varchar(max)",
                unicode: false,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(700)",
                oldUnicode: false,
                oldMaxLength: 700,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShvaTranRecord",
                table: "NayaxTransactionsParameters",
                type: "varchar(max)",
                unicode: false,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(700)",
                oldUnicode: false,
                oldMaxLength: 700,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShvaTranRecord",
                table: "PaymentTransaction",
                type: "varchar(700)",
                unicode: false,
                maxLength: 700,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(max)",
                oldUnicode: false,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShvaTranRecord",
                table: "NayaxTransactionsParameters",
                type: "varchar(700)",
                unicode: false,
                maxLength: 700,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(max)",
                oldUnicode: false,
                oldNullable: true);
        }
    }
}
