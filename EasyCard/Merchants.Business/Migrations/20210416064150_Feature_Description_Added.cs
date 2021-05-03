using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class Feature_Description_Added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionEN",
                table: "Feature",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionHE",
                table: "Feature",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionEN",
                table: "Feature");

            migrationBuilder.DropColumn(
                name: "DescriptionHE",
                table: "Feature");
        }
    }
}
