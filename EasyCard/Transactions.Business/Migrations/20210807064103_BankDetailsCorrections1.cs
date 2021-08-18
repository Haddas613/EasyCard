using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class BankDetailsCorrections1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankTransferDetails_PaymentType",
                table: "PaymentTransaction");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "PaymentTransaction",
                newName: "BankTransferReference");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "PaymentTransaction",
                newName: "BankTransferDueDate");

            migrationBuilder.RenameColumn(
                name: "BankBranch",
                table: "PaymentTransaction",
                newName: "BankTransferBankBranch");

            migrationBuilder.RenameColumn(
                name: "BankAccount",
                table: "PaymentTransaction",
                newName: "BankTransferBankAccount");

            migrationBuilder.RenameColumn(
                name: "Bank",
                table: "PaymentTransaction",
                newName: "BankTransferBank");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BankTransferReference",
                table: "PaymentTransaction",
                newName: "Reference");

            migrationBuilder.RenameColumn(
                name: "BankTransferDueDate",
                table: "PaymentTransaction",
                newName: "DueDate");

            migrationBuilder.RenameColumn(
                name: "BankTransferBankBranch",
                table: "PaymentTransaction",
                newName: "BankBranch");

            migrationBuilder.RenameColumn(
                name: "BankTransferBankAccount",
                table: "PaymentTransaction",
                newName: "BankAccount");

            migrationBuilder.RenameColumn(
                name: "BankTransferBank",
                table: "PaymentTransaction",
                newName: "Bank");

            migrationBuilder.AddColumn<short>(
                name: "BankTransferDetails_PaymentType",
                table: "PaymentTransaction",
                type: "smallint",
                nullable: true);
        }
    }
}
