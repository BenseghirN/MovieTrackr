using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

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

        if (comment.UserId != userId) throw new ForbiddenException();

        comment.Edit(command.Dto.Content);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}