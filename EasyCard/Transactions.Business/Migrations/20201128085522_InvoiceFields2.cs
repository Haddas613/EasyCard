using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class InvoiceFields2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CopyDonwnloadUrl",
                table: "Invoice",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DownloadUrl",
                table: "Invoice",
                type: "varchar(max)",
                unicode: false,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CopyDonwnloadUrl",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "DownloadUrl",
                table: "Invoice");
        }
    }
}
