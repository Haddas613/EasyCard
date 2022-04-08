using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class DealDetails_ExternalFields_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "PaymentTransaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "PaymentTransaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalUserID",
                table: "PaymentTransaction",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePerson",
                table: "PaymentTransaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "PaymentRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "PaymentRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalUserID",
                table: "PaymentRequest",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePerson",
                table: "PaymentRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "Invoice",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Invoice",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalUserID",
                table: "Invoice",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePerson",
                table: "Invoice",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "BillingDeal",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "BillingDeal",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalUserID",
                table: "BillingDeal",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePerson",
                table: "BillingDeal",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ExternalUserID",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "ResponsiblePerson",
                table: "PaymentTransaction");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "ExternalUserID",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "ResponsiblePerson",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "ExternalUserID",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "ResponsiblePerson",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "ExternalUserID",
                table: "BillingDeal");

            migrationBuilder.DropColumn(
                name: "ResponsiblePerson",
                table: "BillingDeal");
        }
    }
}
