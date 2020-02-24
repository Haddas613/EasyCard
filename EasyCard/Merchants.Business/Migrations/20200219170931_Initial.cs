using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Merchants.Business.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalSystem",
                columns: table => new
                {
                    ExternalSystemID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Settings = table.Column<string>(nullable: false),
                    UpdateTimestamp = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalSystem", x => x.ExternalSystemID);
                });

            migrationBuilder.CreateTable(
                name: "Merchant",
                columns: table => new
                {
                    MerchantID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UpdateTimestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    BusinessName = table.Column<string>(maxLength: 50, nullable: false),
                    MarketingName = table.Column<string>(maxLength: 50, nullable: true),
                    BusinessID = table.Column<string>(nullable: true),
                    ContactPerson = table.Column<string>(maxLength: 50, nullable: true),
                    Users = table.Column<string>(unicode: false, nullable: true),
                    Created = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchant", x => x.MerchantID);
                });

            migrationBuilder.CreateTable(
                name: "Terminal",
                columns: table => new
                {
                    TerminalID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantID = table.Column<long>(nullable: false),
                    Label = table.Column<string>(maxLength: 50, nullable: false),
                    UpdateTimestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Status = table.Column<int>(nullable: false),
                    ActivityStartDate = table.Column<DateTime>(nullable: true),
                    Created = table.Column<DateTime>(nullable: true),
                    Settings_RedirectPageSettings = table.Column<string>(nullable: true),
                    Settings_PaymentButtonSettings = table.Column<string>(nullable: true),
                    Settings_MinInstallments = table.Column<int>(nullable: true),
                    Settings_MaxInstallments = table.Column<int>(nullable: true),
                    Settings_MinCreditInstallments = table.Column<int>(nullable: true),
                    Settings_MaxCreditInstallments = table.Column<int>(nullable: true),
                    Settings_EnableDeletionOfUntransmittedTransactions = table.Column<bool>(nullable: true, defaultValue: false),
                    Settings_NationalIDRequired = table.Column<bool>(nullable: true, defaultValue: false),
                    Settings_CvvRequired = table.Column<bool>(nullable: true, defaultValue: false),
                    BillingSettings_BillingNotificationsEmails = table.Column<string>(nullable: true),
                    Users = table.Column<string>(unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terminal", x => x.TerminalID);
                    table.ForeignKey(
                        name: "FK_Terminal_Merchant_MerchantID",
                        column: x => x.MerchantID,
                        principalTable: "Merchant",
                        principalColumn: "MerchantID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feature",
                columns: table => new
                {
                    FeatureID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEN = table.Column<string>(maxLength: 50, nullable: true),
                    NameHE = table.Column<string>(maxLength: 50, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(19,4)", nullable: true, defaultValue: 0m),
                    FeatureCode = table.Column<string>(maxLength: 50, nullable: false),
                    UpdateTimestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    TerminalID = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feature", x => x.FeatureID);
                    table.ForeignKey(
                        name: "FK_Feature_Terminal_TerminalID",
                        column: x => x.TerminalID,
                        principalTable: "Terminal",
                        principalColumn: "TerminalID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TerminalExternalSystem",
                columns: table => new
                {
                    TerminalExternalSystemID = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalSystemID = table.Column<long>(nullable: false),
                    TerminalID = table.Column<long>(nullable: false),
                    ExternalProcessorReference = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Settings = table.Column<string>(nullable: true),
                    UpdateTimestamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminalExternalSystem", x => x.TerminalExternalSystemID);
                    table.ForeignKey(
                        name: "FK_TerminalExternalSystem_ExternalSystem_ExternalSystemID",
                        column: x => x.ExternalSystemID,
                        principalTable: "ExternalSystem",
                        principalColumn: "ExternalSystemID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminalExternalSystem_Terminal_TerminalID",
                        column: x => x.TerminalID,
                        principalTable: "Terminal",
                        principalColumn: "TerminalID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTerminalMappings",
                columns: table => new
                {
                    UserTerminalMappingID = table.Column<string>(nullable: false),
                    UserID = table.Column<string>(nullable: true),
                    TerminalID = table.Column<long>(nullable: false),
                    OperationDate = table.Column<DateTime>(nullable: false),
                    OperationDoneBy = table.Column<string>(nullable: true),
                    OperationDoneByID = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTerminalMappings", x => x.UserTerminalMappingID);
                    table.ForeignKey(
                        name: "FK_UserTerminalMappings_Terminal_TerminalID",
                        column: x => x.TerminalID,
                        principalTable: "Terminal",
                        principalColumn: "TerminalID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feature_TerminalID",
                table: "Feature",
                column: "TerminalID");

            migrationBuilder.CreateIndex(
                name: "IX_Terminal_MerchantID",
                table: "Terminal",
                column: "MerchantID");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalExternalSystem_ExternalSystemID",
                table: "TerminalExternalSystem",
                column: "ExternalSystemID");

            migrationBuilder.CreateIndex(
                name: "IX_TerminalExternalSystem_TerminalID",
                table: "TerminalExternalSystem",
                column: "TerminalID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTerminalMappings_TerminalID",
                table: "UserTerminalMappings",
                column: "TerminalID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feature");

            migrationBuilder.DropTable(
                name: "TerminalExternalSystem");

            migrationBuilder.DropTable(
                name: "UserTerminalMappings");

            migrationBuilder.DropTable(
                name: "ExternalSystem");

            migrationBuilder.DropTable(
                name: "Terminal");

            migrationBuilder.DropTable(
                name: "Merchant");
        }
    }
}
