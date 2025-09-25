using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.UserLists.Commands;

public sealed record DeleteListCommand(Guid UserId, Guid ListId) : IRequest;

public sealed class DeleteListHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<DeleteListCommand>
{
    public async Task Handle(DeleteListCommand command, CancellationToken cancellationToken)
    {
        UserList list = await dbContext.UserLists
            .FirstOrDefaultAsync(l => l.Id == command.ListId && l.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("UserList", command.ListId);

        dbContext.UserLists.Remove(list);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}