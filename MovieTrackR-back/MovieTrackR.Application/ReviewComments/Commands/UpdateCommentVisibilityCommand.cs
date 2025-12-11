using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;

namespace MovieTrackR.Application.ReviewComments.Commands;

public sealed record UpdateCommentVisibilityCommand(Guid Id) : IRequest;

public sealed class UpdateCommentVisibilityHandler(IMovieTrackRDbContext dbContext)
    : IRequestHandler<UpdateCommentVisibilityCommand>
{
    public async Task Handle(UpdateCommentVisibilityCommand command, CancellationToken cancellationToken)
    {
        ReviewComment comment = await dbContext.ReviewComments.FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Comment not found.");

        comment.SetVisibility(!comment.PubliclyVisible);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}