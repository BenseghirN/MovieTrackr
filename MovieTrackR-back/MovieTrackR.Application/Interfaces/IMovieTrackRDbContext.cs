using Microsoft.EntityFrameworkCore;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Interfaces;

public interface IMovieTrackRDbContext
{
    DbSet<User> Users { get; }
    DbSet<Movie> Movies { get; }
    DbSet<Genre> Genres { get; }
    DbSet<Person> People { get; }
    DbSet<Review> Reviews { get; }
    DbSet<ReviewComment> ReviewComments { get; }
    DbSet<ReviewLike> ReviewLikes { get; }
    DbSet<UserList> UserLists { get; }

    // Relation tables
    DbSet<MovieGenre> MovieGenres { get; }
    DbSet<UserListMovie> UserListMovies { get; }
    DbSet<MovieCast> MovieCasts { get; }
    DbSet<MovieCrew> MovieCrews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
