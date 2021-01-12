using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class Feature_Update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feature_Terminal_TerminalID",
                table: "Feature");

            migrationBuilder.DropForeignKey(
                name: "FK_Feature_TerminalTemplate_TerminalTemplateID",
                table: "Feature");

            migrationBuilder.DropIndex(
                name: "IX_Feature_TerminalID",
                table: "Feature");

            migrationBuilder.DropIndex(
                name: "IX_Feature_TerminalTemplateID",
                table: "Feature");

            migrationBuilder.DropColumn(
                name: "FeatureCode",
                table: "Feature");

            migrationBuilder.DropColumn(
                name: "TerminalID",
                table: "Feature");

            migrationBuilder.DropColumn(
                name: "TerminalTemplateID",
                table: "Feature");

            migrationBuilder.AddColumn<string>(
                name: "EnabledFeatures",
                table: "TerminalTemplate",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnabledFeatures",
                table: "Terminal",
                type: "varchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "FeatureID",
                table: "Feature",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnabledFeatures",
                table: "TerminalTemplate");

            migrationBuilder.DropColumn(
                name: "EnabledFeatures",
                table: "Terminal");

            migrationBuilder.AlterColumn<long>(
                name: "FeatureID",
                table: "Feature",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "FeatureCode",
                table: "Feature",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TerminalID",
                table: "Feature",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TerminalTemplateID",
                table: "Feature",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feature_TerminalID",
                table: "Feature",
                column: "TerminalID");

            migrationBuilder.CreateIndex(
                name: "IX_Feature_TerminalTemplateID",
                table: "Feature",
                column: "TerminalTemplateID");

            migrationBuilder.AddForeignKey(
                name: "FK_Feature_Terminal_TerminalID",
                table: "Feature",
                column: "TerminalID",
                principalTable: "Terminal",
                principalColumn: "TerminalID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feature_TerminalTemplate_TerminalTemplateID",
                table: "Feature",
                column: "TerminalTemplateID",
                principalTable: "TerminalTemplate",
                principalColumn: "TerminalTemplateID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
