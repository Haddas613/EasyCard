using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class WoocommerceID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EcwidID",
                table: "Item",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WoocommerceID",
                table: "Item",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EcwidID",
                table: "Consumer",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WoocommerceID",
                table: "Consumer",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EcwidID",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "WoocommerceID",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "EcwidID",
                table: "Consumer");

            migrationBuilder.DropColumn(
                name: "WoocommerceID",
                table: "Consumer");
        }
    }
}
