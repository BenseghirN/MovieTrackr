using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieTrackR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingPropertiesForPeople : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "known_for_department",
                table: "people",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "place_of_birth",
                table: "people",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "known_for_department",
                table: "people");

            migrationBuilder.DropColumn(
                name: "place_of_birth",
                table: "people");
        }
    }
}
