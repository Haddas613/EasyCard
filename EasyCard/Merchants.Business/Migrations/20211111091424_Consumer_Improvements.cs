using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class Consumer_Improvements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumeSecondPhone",
                table: "Consumer");

            migrationBuilder.AlterColumn<string>(
                name: "ConsumerNote",
                table: "Consumer",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerSecondPhone",
                table: "Consumer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumerSecondPhone",
                table: "Consumer");

            migrationBuilder.AlterColumn<string>(
                name: "ConsumerNote",
                table: "Consumer",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumeSecondPhone",
                table: "Consumer",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
