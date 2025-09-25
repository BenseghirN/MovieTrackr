using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.UserLists.Commands;

public sealed record CreateListCommand(Guid UserId, string Title, string Description) : IRequest<Guid>;

public sealed class CreateListHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<CreateListCommand, Guid>
{
    public async Task<Guid> Handle(CreateListCommand request, CancellationToken cancellationToken)
    {
        bool exists = await dbContext.UserLists.AnyAsync(l => l.UserId == request.UserId && l.Title == request.Title, cancellationToken);
        if (exists) throw new ConflictException("List title already exists.");

        UserList list = UserList.Create(request.UserId, request.Title, request.Description);
        dbContext.UserLists.Add(list);
        await dbContext.SaveChangesAsync(cancellationToken);

        return list.Id;
    }
}