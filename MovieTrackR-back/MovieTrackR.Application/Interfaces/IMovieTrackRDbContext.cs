using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    DbSet<MovieProposal> MovieProposals { get; }

    // Relation tables
    DbSet<MovieGenre> MovieGenres { get; }
    DbSet<UserListMovie> UserListMovies { get; }
    DbSet<MovieCast> MovieCasts { get; }
    DbSet<MovieCrew> MovieCrews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    ChangeTracker ChangeTracker { get; }

}
