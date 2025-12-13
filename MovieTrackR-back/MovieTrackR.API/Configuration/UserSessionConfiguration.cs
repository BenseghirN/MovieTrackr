using MovieTrackR.API.Middleware.Contracts;

namespace MovieTrackR.API.Configuration;

public static class UserSessionConfiguration
{
    public static void AddUserSessionConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var sessionOptions = configuration.GetSection("SessionOptions").Get<CustomSessionOptions>();
        services.AddDistributedMemoryCache(); // Pour stocker les sessions en mémoire (peut être remplacé par Redis pour plus d'évolutivité)
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.Parse(sessionOptions.IdleTimeout); // Durée d'inactivité avant expiration
            options.Cookie.HttpOnly = sessionOptions.CookieHttpOnly;
            options.Cookie.IsEssential = sessionOptions.CookieIsEssential;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.None
                    : CookieSecurePolicy.Always;
            options.Cookie.SameSite = environment.IsDevelopment()
                    ? SameSiteMode.Lax // Autorise des requêtes cross-origin simples en dev
                    : SameSiteMode.None;
        });
    }
    public static void UseUserSessionConfiguration(this IApplicationBuilder app)
    {
        app.UseSession();
    }
}