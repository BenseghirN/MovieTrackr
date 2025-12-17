using MovieTrackR.API.Middleware.Contracts;

namespace MovieTrackR.API.Configuration;

public static class UserSessionConfiguration
{
    public static IServiceCollection AddUserSessionConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        CustomSessionOptions sessionOptions = configuration.GetSection("SessionOptions").Get<CustomSessionOptions>() ?? new CustomSessionOptions();
        services.AddDistributedMemoryCache(); // Pour stocker les sessions en mémoire (peut être remplacé par Redis pour plus d'évolutivité)
        services.AddSession(options =>
        {
            TimeSpan idleTimeout = TimeSpan.FromMinutes(30);
            if (!string.IsNullOrWhiteSpace(sessionOptions.IdleTimeout)
                && TimeSpan.TryParse(sessionOptions.IdleTimeout, out var parsed))
            {
                idleTimeout = parsed;
            }

            options.IdleTimeout = idleTimeout; // Durée d'inactivité avant expiration
            options.Cookie.HttpOnly = sessionOptions.CookieHttpOnly;
            options.Cookie.IsEssential = sessionOptions.CookieIsEssential;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.None
                    : CookieSecurePolicy.Always;
            options.Cookie.SameSite = environment.IsDevelopment()
                    ? SameSiteMode.Lax // Autorise des requêtes cross-origin simples en dev
                    : SameSiteMode.None;
        });

        return services;
    }
    public static void UseUserSessionConfiguration(this IApplicationBuilder app)
    {
        app.UseSession();
    }
}