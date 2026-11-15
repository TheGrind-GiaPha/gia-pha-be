using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiaPha.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddImportAndExportStagingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChangeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeRequests_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChangeRequests_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChangeRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExportJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProgressPercentage = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExportJobs_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ExportJobs_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    AcceptedRowsCount = table.Column<int>(type: "int", nullable: false),
                    ConsentGiven = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportBatches_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    ExtractedJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MatchedPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Warnings = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportRows_ImportBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportRows_Persons_MatchedPersonId",
                        column: x => x.MatchedPersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_ClanId",
                table: "ChangeRequests",
                column: "ClanId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_CreatedByUserId",
                table: "ChangeRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_ReviewedByUserId",
                table: "ChangeRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_Status",
                table: "ChangeRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExportJobs_BranchId",
                table: "ExportJobs",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportJobs_ClanId",
                table: "ExportJobs",
                column: "ClanId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportJobs_Status",
                table: "ExportJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_ClanId",
                table: "ImportBatches",
                column: "ClanId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_Status",
                table: "ImportBatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ImportRows_BatchId",
                table: "ImportRows",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportRows_BatchId_RowIndex",
                table: "ImportRows",
                columns: new[] { "BatchId", "RowIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportRows_MatchedPersonId",
                table: "ImportRows",
                column: "MatchedPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportRows_Status",
                table: "ImportRows",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeRequests");

            migrationBuilder.DropTable(
                name: "ExportJobs");

            migrationBuilder.DropTable(
                name: "ImportRows");

            migrationBuilder.DropTable(
                name: "ImportBatches");
        }
    }
}
