using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieTrackR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCastCrewMovieIdIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_movie_crew_movie_id",
                table: "movie_crew",
                column: "movie_id");

            migrationBuilder.CreateIndex(
                name: "ix_movie_cast_movie_id",
                table: "movie_cast",
                column: "movie_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_movie_crew_movie_id",
                table: "movie_crew");

            migrationBuilder.DropIndex(
                name: "ix_movie_cast_movie_id",
                table: "movie_cast");
        }
    }
}
