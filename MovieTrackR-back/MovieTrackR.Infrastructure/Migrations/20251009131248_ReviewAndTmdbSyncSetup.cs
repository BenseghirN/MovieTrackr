using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieTrackR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReviewAndTmdbSyncSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropCheckConstraint(
            //     name: "CK_movie_cast_order_non_negative",
            //     table: "movie_cast");

            migrationBuilder.Sql("""
                ALTER TABLE movie_cast
                DROP CONSTRAINT IF EXISTS "CK_movie_cast_order_non_negative";
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_movie_cast_order_non_negative",
                table: "movie_cast",
                sql: "\"order\" >= 0");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropCheckConstraint(
            //     name: "CK_movie_cast_order_non_negative",
            //     table: "movie_cast");
            migrationBuilder.Sql("""
                ALTER TABLE movie_cast
                DROP CONSTRAINT IF EXISTS "CK_movie_cast_order_non_negative";
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_movie_cast_order_non_negative",
                table: "movie_cast",
                sql: "order >= 0");
        }
    }
}
