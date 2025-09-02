using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Security;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Domain.Entities;
using MovieTrackR.Domain.Enums;

namespace MovieTrackR.API.Configuration;

public static class AuthorizationConfiguration
{
    public const string AdminPolicy = "AdminRolePolicy";
    public const string ReviewOwnerPolicy = "IsReviewOwner";
    public const string ListOwnerPolicy = "IsListOwner";

    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        // Handlers
        services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, OwnerAuthorizationHandler>();

        // Policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AdminPolicy, policyBuilder =>
                policyBuilder.RequireAuthenticatedUser()
                 .AddRequirements(new RoleRequirement(UserRole.Admin)));

            options.AddPolicy(ReviewOwnerPolicy, policyBuilder =>
                policyBuilder.RequireAuthenticatedUser()
                 .AddRequirements(new OwnerRequirement(ResourceKind.Review)));

            options.AddPolicy(ListOwnerPolicy, policyBuilder =>
                policyBuilder.RequireAuthenticatedUser()
                 .AddRequirements(new OwnerRequirement(ResourceKind.UserList)));
        });

        return services;
    }
}


public sealed record RoleRequirement(UserRole Role) : IAuthorizationRequirement;
public sealed class RoleAuthorizationHandler(IMovieTrackRDbContext dbContext) : AuthorizationHandler<RoleRequirement>()
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        string? externalId = ClaimsExtensions.GetExternalId(context.User);
        if (string.IsNullOrWhiteSpace(externalId))
            return;
        User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.ExternalId == externalId);

        if (user != null && user.Role == requirement.Role)
        {
            context.Succeed(requirement);
        }
    }
}

public sealed record OwnerRequirement(ResourceKind Kind, string RouteParam = "id") : IAuthorizationRequirement;
public sealed class OwnerAuthorizationHandler(IMovieTrackRDbContext db) : AuthorizationHandler<OwnerRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnerRequirement requirement)
    {
        string? externalId = ClaimsExtensions.GetExternalId(context.User);
        if (string.IsNullOrWhiteSpace(externalId)) return;

        User? user = await db.Users.FirstOrDefaultAsync(u => u.ExternalId == externalId);
        if (user is null) return;

        // Bypass if user is Admin
        if (user.Role == UserRole.Admin)
        {
            context.Succeed(requirement);
            return;
        }

        HttpContext? httpCtx = (context.Resource as HttpContext) ??
                      (context.Resource as DefaultHttpContext);
        string? idStr = httpCtx?.GetRouteValue(requirement.RouteParam)?.ToString();
        if (!Guid.TryParse(idStr, out Guid resourceId)) return;

        bool isOwner = requirement.Kind switch
        {
            ResourceKind.Review => await db.Reviews
                .AnyAsync(r => r.Id == resourceId && r.UserId == user.Id),

            ResourceKind.UserList => await db.UserLists
                .AnyAsync(l => l.Id == resourceId && l.UserId == user.Id),

            _ => false
        };

        if (isOwner) context.Succeed(requirement);
    }
}
