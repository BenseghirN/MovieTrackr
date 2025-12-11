using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Application.ReviewComments.Commands;

public sealed record UpdateCommentCommand(Guid ReviewId, Guid CommentId, CommentUpdateDto Dto, CurrentUserDto CurrentUser) : IRequest;

public sealed class UpdateCommentHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<UpdateCommentCommand>
{
    public async Task Handle(UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);

        ReviewComment comment = await dbContext.ReviewComments.FirstOrDefaultAsync(x => x.Id == command.CommentId && x.ReviewId == command.ReviewId, cancellationToken)
            ?? throw new NotFoundException("Comment not found.");

        UserRole userRole = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Role)
            .FirstAsync(cancellationToken);

        bool isAdmin = userRole == UserRole.Admin;

        if (comment.UserId != userId && !isAdmin) throw new ForbiddenException();

        comment.Edit(command.Dto.Content);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}