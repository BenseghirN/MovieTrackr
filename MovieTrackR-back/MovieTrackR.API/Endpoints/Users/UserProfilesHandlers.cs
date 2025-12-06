using MediatR;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Users.Queries;

namespace MovieTrackR.API.Endpoints.Users;

public static class UserProfilesHandlers
{
    // public static async Task<IResult> GetAll(IMediator mediator, CancellationToken cancellationToken)
    // {
    //     List<UserDto> list = await mediator.Send(new GetAllUsersQuery(), cancellationToken);
    //     return TypedResults.Ok(list);
    // }

    public static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        PublicUserProfileDto? dto = await mediator.Send(new GetPublicUserProfileQuery(id), cancellationToken);
        return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
    }
}