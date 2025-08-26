using System.Threading.RateLimiting;

namespace MovieTrackR.API.Configuration;

public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // 60 req/min per user (if logged off => per IP)
            options.AddPolicy("per-user", ctx =>
            {
                var key =
                    ctx.User.Identity?.IsAuthenticated == true
                        ? (ctx.User.Identity!.Name
                           ?? ctx.User.FindFirst("sub")?.Value
                           ?? "auth-unknown")
                        : ctx.Connection.RemoteIpAddress?.ToString() ?? "ip-unknown";

                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });

            // more strict for login : 10 req/min par IP
            options.AddPolicy("login", ctx =>
            {
                var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "ip-unknown";
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });
        });

        return services;
    }

    public static WebApplication UseRateLimitingConfiguration(this WebApplication app)
    {
        app.UseRateLimiter();
        return app;
    }
}