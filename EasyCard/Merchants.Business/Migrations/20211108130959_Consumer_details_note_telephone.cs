using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class Consumer_details_note_telephone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsumeSecondPhone",
                table: "Consumer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsumerNote",
                table: "Consumer",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumeSecondPhone",
                table: "Consumer");

            migrationBuilder.DropColumn(
                name: "ConsumerNote",
                table: "Consumer");
        }
    }
}
