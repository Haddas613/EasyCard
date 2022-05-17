using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Transactions.Business.Migrations
{
    public partial class ThreeDSChallenge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ThreeDSChallengeID",
                table: "PaymentTransaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ThreeDSChallenge",
                columns: table => new
                {
                    ThreeDSChallengeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageDate = table.Column<DateTime>(type: "date", nullable: true),
                    MessageTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Action = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    ThreeDSServerTransID = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TerminalID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MerchantID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransStatus = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreeDSChallenge", x => x.ThreeDSChallengeID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThreeDSChallenge_ThreeDSServerTransID",
                table: "ThreeDSChallenge",
                column: "ThreeDSServerTransID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThreeDSChallenge");

            migrationBuilder.DropColumn(
                name: "ThreeDSChallengeID",
                table: "PaymentTransaction");
        }
    }
}
