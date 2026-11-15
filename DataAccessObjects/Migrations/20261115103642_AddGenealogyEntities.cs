using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiaPha.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddGenealogyEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginStory",
                table: "Clans",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Clans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rules",
                table: "Clans",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Clans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FounderPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HuyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TuName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ThuyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    BirthDate_Year = table.Column<int>(type: "int", nullable: true),
                    BirthDate_Month = table.Column<int>(type: "int", nullable: true),
                    BirthDate_Day = table.Column<int>(type: "int", nullable: true),
                    BirthDate_Calendar = table.Column<int>(type: "int", nullable: true),
                    BirthDate_IsLeapMonth = table.Column<bool>(type: "bit", nullable: true),
                    DeathDate_Year = table.Column<int>(type: "int", nullable: true),
                    DeathDate_Month = table.Column<int>(type: "int", nullable: true),
                    DeathDate_Day = table.Column<int>(type: "int", nullable: true),
                    DeathDate_Calendar = table.Column<int>(type: "int", nullable: true),
                    DeathDate_IsLeapMonth = table.Column<bool>(type: "bit", nullable: true),
                    IsLiving = table.Column<bool>(type: "bit", nullable: false),
                    Generation = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PrivacyLevel = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Persons_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParentChild",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelationType = table.Column<int>(type: "int", nullable: false),
                    SiblingOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentChild", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentChild_Persons_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParentChild_Persons_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Unions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HusbandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WifeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Unions_Persons_HusbandId",
                        column: x => x.HusbandId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Unions_Persons_WifeId",
                        column: x => x.WifeId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Clans",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "OriginStory", "PublishedAt", "Rules" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Clans",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "OriginStory", "PublishedAt", "Rules" },
                values: new object[] { null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ClanId",
                table: "Branches",
                column: "ClanId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_FounderPersonId",
                table: "Branches",
                column: "FounderPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentChild_ChildId",
                table: "ParentChild",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentChild_ParentId_ChildId",
                table: "ParentChild",
                columns: new[] { "ParentId", "ChildId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_BranchId",
                table: "Persons",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_ClanId",
                table: "Persons",
                column: "ClanId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Path",
                table: "Persons",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_Unions_HusbandId_WifeId",
                table: "Unions",
                columns: new[] { "HusbandId", "WifeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unions_WifeId",
                table: "Unions",
                column: "WifeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Persons_FounderPersonId",
                table: "Branches",
                column: "FounderPersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Persons_FounderPersonId",
                table: "Branches");

            migrationBuilder.DropTable(
                name: "ParentChild");

            migrationBuilder.DropTable(
                name: "Unions");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropColumn(
                name: "OriginStory",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "Rules",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Clans");
        }
    }
}
