using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieTrackR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieProposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "movie_proposals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1mc()"),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    original_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    release_year = table.Column<int>(type: "integer", nullable: true),
                    country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    overview = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    poster_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    proposed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    moderation_note = table.Column<string>(type: "text", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movie_proposals", x => x.id);
                    table.CheckConstraint("CK_movie_proposals_release_year", "release_year IS NULL OR (release_year >= 1888 AND release_year <= EXTRACT(YEAR FROM CURRENT_DATE)::int + 1)");
                    table.ForeignKey(
                        name: "fk_movie_proposals_users_proposed_by_user_id",
                        column: x => x.proposed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_movie_proposals_proposed_by_user_id",
                table: "movie_proposals",
                column: "proposed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_movie_proposals_status",
                table: "movie_proposals",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movie_proposals");
        }
    }
}
