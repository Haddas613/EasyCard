using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class SettingsRevisited : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingNotificationsEmails",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "CvvRequired",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "EnableDeletionOfUntransmittedTransactions",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "J2Allowed",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "J5Allowed",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "NationalIDRequired",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "PaymentButtonSettings",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "RedirectPageSettings",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "Settings_MaxCreditInstallments",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "Settings_MaxInstallments",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "Settings_MinCreditInstallments",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "Settings_MinInstallments",
                table: "Terminal");

            migrationBuilder.AddColumn<string>(
                name: "BillingSettings",
                table: "Terminal",
                type: "nvarchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutSettings",
                table: "Terminal",
                type: "nvarchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceSettings",
                table: "Terminal",
                type: "nvarchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentRequestSettings",
                table: "Terminal",
                type: "nvarchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Settings",
                table: "Terminal",
                type: "nvarchar(max)",
                unicode: false,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    SystemSettingsID = table.Column<int>(type: "int", nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", unicode: false, nullable: true),
                    BillingSettings = table.Column<string>(type: "nvarchar(max)", unicode: false, nullable: true),
                    InvoiceSettings = table.Column<string>(type: "nvarchar(max)", unicode: false, nullable: true),
                    PaymentRequestSettings = table.Column<string>(type: "nvarchar(max)", unicode: false, nullable: true),
                    CheckoutSettings = table.Column<string>(type: "nvarchar(max)", unicode: false, nullable: true),
                    UpdateTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.SystemSettingsID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "BillingSettings",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "CheckoutSettings",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "InvoiceSettings",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "PaymentRequestSettings",
                table: "Terminal");

            migrationBuilder.DropColumn(
                name: "Settings",
                table: "Terminal");

            migrationBuilder.AddColumn<string>(
                name: "BillingNotificationsEmails",
                table: "Terminal",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CvvRequired",
                table: "Terminal",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableDeletionOfUntransmittedTransactions",
                table: "Terminal",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "J2Allowed",
                table: "Terminal",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "J5Allowed",
                table: "Terminal",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NationalIDRequired",
                table: "Terminal",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentButtonSettings",
                table: "Terminal",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RedirectPageSettings",
                table: "Terminal",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Settings_MaxCreditInstallments",
                table: "Terminal",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Settings_MaxInstallments",
                table: "Terminal",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Settings_MinCreditInstallments",
                table: "Terminal",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Settings_MinInstallments",
                table: "Terminal",
                type: "int",
                nullable: true);
        }
    }
}
