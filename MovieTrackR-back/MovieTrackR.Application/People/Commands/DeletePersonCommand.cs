using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.People.Commands;

public sealed record DeletePersonCommand(Guid Id) : IRequest;

public sealed class DeletePersonHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<DeletePersonCommand>
{
    public async Task Handle(DeletePersonCommand deleteCommand, CancellationToken cancellationToken)
    {
        Person person = await dbContext.People.FirstOrDefaultAsync(x => x.Id == deleteCommand.Id, cancellationToken)
            ?? throw new NotFoundException("Movie not found.");
        dbContext.People.Remove(person);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}