using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiaPha.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonHasHiddenChildren : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasHiddenChildren",
                table: "Persons",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasHiddenChildren",
                table: "Persons");
        }
    }
}
