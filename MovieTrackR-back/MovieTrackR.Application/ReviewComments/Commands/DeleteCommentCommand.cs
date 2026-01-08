using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.ReviewComments.Commands;

public sealed record DeleteCommentCommand(Guid ReviewId, Guid CommentId, CurrentUserDto CurrentUser) : IRequest;


public sealed class DeleteCommentHandler(IMovieTrackRDbContext dbContext, IMediator mediator)
    : IRequestHandler<DeleteCommentCommand>
{
    public async Task Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await mediator.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);

        ReviewComment comment = await dbContext.ReviewComments.FirstOrDefaultAsync(x => x.Id == command.CommentId && x.ReviewId == command.ReviewId, cancellationToken)
            ?? throw new NotFoundException("Comment not found.");

        if (comment.UserId != userId) throw new ForbiddenException();

        dbContext.ReviewComments.Remove(comment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}