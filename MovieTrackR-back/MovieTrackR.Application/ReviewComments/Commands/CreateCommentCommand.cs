using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Commands;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.ReviewComments.Commands;

public sealed record CreateCommentCommand(Guid ReviewId, CommentCreateDto Dto, CurrentUserDto CurrentUser) : IRequest<Guid>;

public sealed class CreateCommentHandler(IMovieTrackRDbContext dbContext, ISender sender)
    : IRequestHandler<CreateCommentCommand, Guid>
{
    public async Task<Guid> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
    {
        Guid userId = await sender.Send(new EnsureUserExistsCommand(command.CurrentUser), cancellationToken);

        bool reviewExists = await dbContext.Reviews.AnyAsync(r => r.Id == command.ReviewId, cancellationToken);
        if (!reviewExists) throw new NotFoundException("Review not found.");

        ReviewComment comment = ReviewComment.Create(
            command.ReviewId, userId, command.Dto.Content);

        dbContext.ReviewComments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return comment.Id;
    }
}