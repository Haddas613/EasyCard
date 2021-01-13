using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class Feature_Update1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feature_Terminal_TerminalID",
                table: "Feature");

            migrationBuilder.DropForeignKey(
                name: "FK_Feature_TerminalTemplate_TerminalTemplateID",
                table: "Feature");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Feature",
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
                name: "FeatureID",
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

            migrationBuilder.AddColumn<short>(
                name: "FeatureIDTMP",
                table: "Feature",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Feature",
                table: "Feature",
                column: "FeatureIDTMP");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Feature",
                table: "Feature");

            migrationBuilder.DropColumn(
                name: "EnabledFeatures",
                table: "TerminalTemplate");

            migrationBuilder.DropColumn(
                name: "EnabledFeatures",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "FeatureIDTMP",
                table: "Feature");

            migrationBuilder.AddColumn<string>(
                name: "FeatureCode",
                table: "Feature",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FeatureID",
                table: "Feature",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_Feature",
                table: "Feature",
                column: "FeatureID");

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
