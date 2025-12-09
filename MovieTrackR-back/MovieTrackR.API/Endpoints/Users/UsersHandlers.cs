using System.Security.Claims;
using MediatR;
using MovieTrackR.API.Middleware;
using MovieTrackR.Application.DTOs;
using MovieTrackR.Application.Users.Commands;
using MovieTrackR.Application.Users.Queries;

namespace MovieTrackR.API.Endpoints.Users;

public static class UsersHandlers
{
    public static async Task<IResult> GetAll(IMediator mediator, CancellationToken cancellationToken)
    {
        List<UserDto> list = await mediator.Send(new GetAllUsersQuery(), cancellationToken);
        return TypedResults.Ok(list);
    }

    public static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        UserDto? dto = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
    }

    public static async Task<IResult> PromoteToAdmin(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        UserDto dto = await mediator.Send(new PromoteToAdminCommand(id), cancellationToken);
        return TypedResults.Ok(dto);
    }

    public static async Task<IResult> DemoteToUser(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        UserDto dto = await mediator.Send(new DemoteToUserCommand(id), cancellationToken);
        return TypedResults.Ok(dto);
    }

    public static async Task<IResult> UpdateUser(Guid id, ClaimsPrincipal user, UserUpdateDto updatedUser, IMediator mediator, CancellationToken cancellationToken)
    {
        CurrentUserDto current = user.ToCurrentUserDto();
        PublicUserProfileDto dto = await mediator.Send(new UpdateUserInfosCommand(id, current, updatedUser), cancellationToken);
        return TypedResults.Ok(dto);
    }

    public static async Task<IResult> UpdateUserAvatar(Guid id, ClaimsPrincipal user, HttpRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        var (success, error, content, contentType) = await ExtractPictureAsync(request, cancellationToken);
        if (!success)
            return error!;

        CurrentUserDto current = user.ToCurrentUserDto();
        PublicUserProfileDto dto = await mediator.Send(new UpdateUserAvatarCommand(id, current, content!, contentType!), cancellationToken);
        return TypedResults.Ok(dto);
    }

    private static async Task<(bool success, IResult? error, byte[]? content, string? contentType)> ExtractPictureAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
            return (false, TypedResults.BadRequest("Form-data content type is required."), null, null);

        IFormCollection form = await request.ReadFormAsync(cancellationToken);
        IFormFile? file = form.Files["avatar"];
        if (file is null || file.Length == 0)
            return (false, TypedResults.BadRequest("No file provided."), null, null);

        string[] allowed = { "image/jpeg", "image/png", "image/webp" };
        if (Array.IndexOf(allowed, file.ContentType) == -1)
            return (false, TypedResults.BadRequest("Unsupported file type."), null, null);

        // optionnel : limiter la taille (ex: 2 Mo)
        if (file.Length > 2 * 1024 * 1024)
            return (false, TypedResults.BadRequest("File too large."), null, null);

        using MemoryStream ms = new();
        await file.CopyToAsync(ms, cancellationToken);

        return (true, null, ms.ToArray(), file.ContentType);
    }
}