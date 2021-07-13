using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class CompRetailerNum_N_EmvSoftVersion_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompRetailerNum",
                table: "PaymentTransaction",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmvSoftVersion",
                table: "PaymentTransaction",
                type: "nvarchar(50)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompRetailerNum",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "EmvSoftVersion",
                table: "PaymentTransaction");
        }
    }
}
