using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Application.ReviewComments.Commands;

public sealed record DeleteCommentCommand(Guid ReviewId, Guid CommentId, CurrentUserDto CurrentUser) : IRequest;


public sealed class DeleteCommentHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<DeleteCommentCommand>
{
    public async Task Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);

        var c = await dbContext.ReviewComments.FirstOrDefaultAsync(x => x.Id == command.CommentId && x.ReviewId == command.ReviewId, cancellationToken)
            ?? throw new NotFoundException("Comment not found.");

        if (c.UserId != userId) throw new ForbiddenException();

        dbContext.ReviewComments.Remove(c);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}