using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiaPha.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonProfileFields : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
