using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiaPha.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeRequestsMagicLinksAndProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Biography",
                table: "Persons",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BurialMapUrl",
                table: "Persons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BurialPlace",
                table: "Persons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Persons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherNames",
                table: "Persons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChangeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DueAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MagicLinkTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MagicLinkTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_ClanId",
                table: "ChangeRequests",
                column: "ClanId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_ClanId_Status",
                table: "ChangeRequests",
                columns: new[] { "ClanId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MagicLinkTokens_TokenHash",
                table: "MagicLinkTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeRequests");

            migrationBuilder.DropTable(
                name: "MagicLinkTokens");

            migrationBuilder.DropColumn(
                name: "Biography",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "BurialMapUrl",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "BurialPlace",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "OtherNames",
                table: "Persons");
        }
    }
}
