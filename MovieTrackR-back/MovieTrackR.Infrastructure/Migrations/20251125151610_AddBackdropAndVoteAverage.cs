using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieTrackR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBackdropAndVoteAverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "backdrop_path",
                table: "movies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "vote_average",
                table: "movies",
                type: "double precision",
                precision: 3,
                scale: 1,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "backdrop_path",
                table: "movies");

            migrationBuilder.DropColumn(
                name: "vote_average",
                table: "movies");
        }
    }
}
