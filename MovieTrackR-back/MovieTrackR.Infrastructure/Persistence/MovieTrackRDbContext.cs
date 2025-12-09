using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Infrastructure.Persistence;

public class MovieTrackRDbContext : DbContext, IMovieTrackRDbContext
{
    public MovieTrackRDbContext(DbContextOptions<MovieTrackRDbContext> options) : base(options) { }

    // Main
    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();
    public DbSet<ReviewLike> ReviewLikes => Set<ReviewLike>();
    public DbSet<UserList> UserLists => Set<UserList>();
    public DbSet<MovieProposal> MovieProposals => Set<MovieProposal>();

    // Relations
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<UserListMovie> UserListMovies => Set<UserListMovie>();
    public DbSet<MovieCast> MovieCasts => Set<MovieCast>();
    public DbSet<MovieCrew> MovieCrews => Set<MovieCrew>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.UseUtcDateTimes();

        //  Users 
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.ExternalId).IsUnique();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.Role).HasConversion(new EnumToStringConverter<UserRole>());
        });

        //  Movies 
        b.Entity<Movie>(e =>
        {
            e.ToTable("movies");
            e.HasIndex(x => x.TmdbId).IsUnique().HasFilter("\"TmdbId\" IS NOT NULL");
            e.HasIndex(x => x.CreatedAt);
            e.Property(x => x.ReleaseDate).HasColumnType("date");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.BackdropPath).HasMaxLength(500);
            e.Property(x => x.VoteAverage).HasPrecision(3, 1);
        });

        //  Genres 
        b.Entity<Genre>(e =>
        {
            e.ToTable("genres");
            e.HasIndex(x => x.TmdbId).IsUnique();
            e.HasIndex(x => x.Name).IsUnique();
        });

        //  MovieGenres (composite PK) 
        b.Entity<MovieGenre>(e =>
        {
            e.ToTable("movie_genres");
            e.HasKey(x => new { x.MovieId, x.GenreId });
            e.HasOne(x => x.Movie).WithMany(m => m.MovieGenres).HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Genre).WithMany(g => g.MovieGenres).HasForeignKey(x => x.GenreId).OnDelete(DeleteBehavior.Cascade);
        });

        //  Reviews 
        b.Entity<Review>(e =>
        {
            e.ToTable("reviews", t => t.HasCheckConstraint("CK_reviews_rating_range", "rating >= 0 AND rating <= 5"));
            e.HasIndex(x => new { x.UserId, x.MovieId }).IsUnique();
            e.HasIndex(x => x.MovieId);
            e.HasIndex(x => x.UserId);
            e.Property(x => x.Rating).HasColumnType("real");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasOne(x => x.User).WithMany(u => u.Reviews).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Movie).WithMany(m => m.Reviews).HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Cascade);
        });

        //  ReviewComments 
        b.Entity<ReviewComment>(e =>
        {
            e.ToTable("review_comments");
            e.HasIndex(x => new { x.ReviewId, x.CreatedAt });
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasOne(x => x.Review).WithMany(r => r.Comments).HasForeignKey(x => x.ReviewId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany(u => u.ReviewComments).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        //  ReviewLikes (composite PK) 
        b.Entity<ReviewLike>(e =>
        {
            e.ToTable("review_likes");
            e.HasKey(x => new { x.ReviewId, x.UserId });
            e.HasIndex(x => x.ReviewId);
            e.HasIndex(x => x.UserId);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasOne(x => x.Review).WithMany(r => r.Likes).HasForeignKey(x => x.ReviewId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany(u => u.ReviewLikes).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        //  UserLists 
        b.Entity<UserList>(e =>
        {
            e.ToTable("user_lists");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.HasOne(x => x.User).WithMany(u => u.Lists).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        //  UserListMovies (composite PK + position) 
        b.Entity<UserListMovie>(e =>
        {
            e.ToTable("user_list_movies", t => t.HasCheckConstraint("CK_user_list_movies_position_gt_zero", "position > 0"));
            e.HasKey(x => new { x.UserListId, x.MovieId });
            e.Property(x => x.Position).IsRequired();
            e.HasIndex(x => new { x.UserListId, x.Position }).IsUnique();
            e.HasOne(x => x.UserList).WithMany(l => l.Movies).HasForeignKey(x => x.UserListId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Movie).WithMany().HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Cascade);
        });

        //  People 
        b.Entity<Person>(e =>
        {
            e.ToTable("people");
            e.HasIndex(x => x.TmdbId).IsUnique();
            e.Property(x => x.BirthDate).HasColumnType("date");
            e.Property(x => x.DeathDate).HasColumnType("date");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        //  MovieCast 
        b.Entity<MovieCast>(e =>
        {
            e.ToTable("movie_cast", t => t.HasCheckConstraint("CK_movie_cast_order_non_negative", "\"order\" >= 0"));
            e.Property(x => x.Order).HasColumnName("order"); // mot réservé
            e.HasIndex(x => new { x.MovieId, x.PersonId });
            e.HasOne(x => x.Movie).WithMany(m => m.Cast).HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Person).WithMany(p => p.CastRoles).HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Cascade);
        });

        //  MovieCrew 
        b.Entity<MovieCrew>(e =>
        {
            e.ToTable("movie_crew");
            e.HasIndex(x => new { x.MovieId, x.PersonId, x.Department, x.Job }).IsUnique();
            e.HasOne(x => x.Movie).WithMany(m => m.Crew).HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Person).WithMany(p => p.CrewRoles).HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Cascade);
        });

        // MovieProposals
        b.Entity<MovieProposal>(e =>
        {
            e.ToTable("movie_proposals",
                t => t.HasCheckConstraint("CK_movie_proposals_release_year",
                "release_year IS NULL OR (release_year >= 1888 AND release_year <= EXTRACT(YEAR FROM CURRENT_DATE)::int + 1)"));
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.OriginalTitle).HasMaxLength(200);
            e.Property(x => x.Country).HasMaxLength(2);
            e.Property(x => x.PosterUrl).HasMaxLength(2048);
            e.Property(x => x.Overview).HasMaxLength(2000);
            e.Property(x => x.Status).HasConversion(new EnumToStringConverter<MovieProposalStatus>());

            e.HasIndex(x => x.ProposedByUserId);
            e.HasIndex(x => x.Status);

            e.HasOne(x => x.ProposedByUser)
            .WithMany()
            .HasForeignKey(x => x.ProposedByUserId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        b.UseSequentialGuids();   // => DEFAULT uuid_generate_v1mc() on PK GUID
    }
}
