using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class R1IntegrationCorrections2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExternalConsumerCode",
                table: "PaymentTransaction",
                newName: "ConsumerExternalReference");

            migrationBuilder.RenameColumn(
                name: "ExternalConsumerCode",
                table: "PaymentRequest",
                newName: "ConsumerExternalReference");

            migrationBuilder.RenameColumn(
                name: "ExternalConsumerCode",
                table: "Invoice",
                newName: "ConsumerExternalReference");

            migrationBuilder.RenameColumn(
                name: "ExternalConsumerCode",
                table: "BillingDeal",
                newName: "ConsumerExternalReference");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConsumerExternalReference",
                table: "PaymentTransaction",
                newName: "ExternalConsumerCode");

            migrationBuilder.RenameColumn(
                name: "ConsumerExternalReference",
                table: "PaymentRequest",
                newName: "ExternalConsumerCode");

            migrationBuilder.RenameColumn(
                name: "ConsumerExternalReference",
                table: "Invoice",
                newName: "ExternalConsumerCode");

            migrationBuilder.RenameColumn(
                name: "ConsumerExternalReference",
                table: "BillingDeal",
                newName: "ExternalConsumerCode");
        }
    }
}
