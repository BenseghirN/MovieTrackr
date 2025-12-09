using MediatR;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Users.Queries;

namespace MovieTrackR.API.Endpoints.Users;

public static class UserProfilesHandlers
{
    public static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        PublicUserProfileDto? dto = await mediator.Send(new GetPublicUserProfileQuery(id), cancellationToken);
        return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
    }

    public static async Task<IResult> GetListsByUser(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        IReadOnlyList<UserListDto> result = await sender.Send(new GetListsByUserIdQuery(id), cancellationToken);
        return TypedResults.Ok(result);
    }
}