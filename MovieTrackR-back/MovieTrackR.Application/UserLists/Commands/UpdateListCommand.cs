using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.UserLists.Commands;

public sealed record UpdateListCommand(CurrentUserDto currentUser, Guid ListId, string Title, string Description) : IRequest<UserListDto>;

public sealed class UpdateListHandler(IMovieTrackRDbContext dbContext, IMapper mapper, IMediator mediator) : IRequestHandler<UpdateListCommand, UserListDto>
{
    public async Task<UserListDto> Handle(UpdateListCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await mediator.Send(new EnsureUserExistsCommand(command.currentUser), cancellationToken);

        UserList list = await dbContext.UserLists.FirstOrDefaultAsync(l => l.Id == command.ListId && l.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Userlist", command.ListId);

        bool exists = await dbContext.UserLists.AnyAsync(l => l.UserId == userId && l.Title == command.Title, cancellationToken);
        if (exists) throw new ConflictException("List title already exists.");

        list.Update(command.Title, command.Description);
        await dbContext.SaveChangesAsync(cancellationToken);

        return mapper.Map<UserListDto>(list);
    }
}