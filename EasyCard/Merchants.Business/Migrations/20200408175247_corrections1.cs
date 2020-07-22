using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
#pragma warning disable SA1300 // Element should begin with upper-case letter
    public partial class corrections1 : Migration
#pragma warning restore SA1300 // Element should begin with upper-case letter
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InstanceTypeFullName",
                table: "ExternalSystem",
                unicode: false,
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(512)",
                oldUnicode: false,
                oldMaxLength: 512);

            migrationBuilder.AddColumn<string>(
                name: "SettingsTypeFullName",
                table: "ExternalSystem",
                unicode: false,
                maxLength: 512,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SettingsTypeFullName",
                table: "ExternalSystem");

            migrationBuilder.AlterColumn<string>(
                name: "InstanceTypeFullName",
                table: "ExternalSystem",
                type: "varchar(512)",
                unicode: false,
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldUnicode: false,
                oldMaxLength: 512,
                oldNullable: true);
        }
    }
}
