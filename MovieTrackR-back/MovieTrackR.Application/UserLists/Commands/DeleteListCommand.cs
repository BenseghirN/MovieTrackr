using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.UserLists.Commands;

public sealed record DeleteListCommand(CurrentUserDto CurrentUser, Guid ListId) : IRequest;

public sealed class DeleteListHandler(IMovieTrackRDbContext dbContext, IMediator mediator)
    : IRequestHandler<DeleteListCommand>
{
    public async Task Handle(DeleteListCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await mediator.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);
        UserList list = await dbContext.UserLists
            .FirstOrDefaultAsync(l => l.Id == command.ListId && l.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("UserList", command.ListId);

        if (list.IsSystemList)
            throw new ForbiddenException("Cannot delete system lists");

        dbContext.UserLists.Remove(list);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}