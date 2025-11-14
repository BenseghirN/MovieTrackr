using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.Movies.Commands;

public sealed record DeleteMovieCommand(Guid Id) : IRequest;

public sealed class DeleteMovieHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<DeleteMovieCommand>
{
    public async Task Handle(DeleteMovieCommand deleteCommand, CancellationToken cancellationToken)
    {
        Movie movie = await dbContext.Movies.FirstOrDefaultAsync(x => x.Id == deleteCommand.Id, cancellationToken)
            ?? throw new NotFoundException("Movie not found.");
        dbContext.Movies.Remove(movie);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}