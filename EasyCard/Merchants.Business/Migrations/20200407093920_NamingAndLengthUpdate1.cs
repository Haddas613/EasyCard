using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class NamingAndLengthUpdate1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalDetails",
                table: "MerchantHistory");

            migrationBuilder.RenameColumn(
                name: "Settings_RedirectPageSettings",
                table: "Terminal",
                newName: "RedirectPageSettings");

            migrationBuilder.RenameColumn(
                name: "Settings_PaymentButtonSettings",
                table: "Terminal",
                newName: "PaymentButtonSettings");

            migrationBuilder.RenameColumn(
                name: "Settings_NationalIDRequired",
                table: "Terminal",
                newName: "NationalIDRequired");

            migrationBuilder.RenameColumn(
                name: "Settings_EnableDeletionOfUntransmittedTransactions",
                table: "Terminal",
                newName: "EnableDeletionOfUntransmittedTransactions");

            migrationBuilder.RenameColumn(
                name: "Settings_CvvRequired",
                table: "Terminal",
                newName: "CvvRequired");

            migrationBuilder.RenameColumn(
                name: "BillingSettings_BillingNotificationsEmails",
                table: "Terminal",
                newName: "BillingNotificationsEmails");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RedirectPageSettings",
                table: "Terminal",
                newName: "Settings_RedirectPageSettings");

            migrationBuilder.RenameColumn(
                name: "PaymentButtonSettings",
                table: "Terminal",
                newName: "Settings_PaymentButtonSettings");

            migrationBuilder.RenameColumn(
                name: "NationalIDRequired",
                table: "Terminal",
                newName: "Settings_NationalIDRequired");

            migrationBuilder.RenameColumn(
                name: "EnableDeletionOfUntransmittedTransactions",
                table: "Terminal",
                newName: "Settings_EnableDeletionOfUntransmittedTransactions");

            migrationBuilder.RenameColumn(
                name: "CvvRequired",
                table: "Terminal",
                newName: "Settings_CvvRequired");

            migrationBuilder.RenameColumn(
                name: "BillingNotificationsEmails",
                table: "Terminal",
                newName: "BillingSettings_BillingNotificationsEmails");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalDetails",
                table: "MerchantHistory",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
