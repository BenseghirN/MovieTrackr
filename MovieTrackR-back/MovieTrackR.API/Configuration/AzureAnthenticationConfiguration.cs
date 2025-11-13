using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MovieTrackR.Domain.Enums;

namespace MovieTrackR.API.Configuration;

public static class AzureAnthenticationConfiguration
{
    public static IServiceCollection AddAzureAnthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection azureB2CSection = configuration.GetSection("EntraExternalId");
        // string authority = azureB2CSection["Authority"] ?? throw new ArgumentNullException("Authority");
        string? authority = azureB2CSection["Authority"];
        if (string.IsNullOrWhiteSpace(authority) || authority.Contains("fake"))
        {
            // Auth de test sera configurée ailleurs (TestAppFactory)
            return services;
        }
        string clientId = azureB2CSection["ClientId"] ?? throw new ArgumentNullException("ClientId");
        string clientSecret = azureB2CSection["clientSecret"] ?? throw new ArgumentNullException("clientSecret");
        string callbackPath = azureB2CSection["CallbackPath"] ?? throw new ArgumentNullException("CallbackPath");
        string OpenIdScheme = azureB2CSection["OpenIdScheme"] ?? throw new ArgumentNullException("OpenIdScheme");

        services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                    })
                .AddCookie(options =>
                    {
                        options.Cookie.Name = "MovieTrackR_auth";         // Custom cookie name
                        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Use Secure cookies in production (HTTPS)
                        options.Cookie.SameSite = SameSiteMode.Strict;  // Prevent CSRF attacks
                        options.Cookie.HttpOnly = true;                 // Prevent JS access (XSS protection)
                        options.LoginPath = "/api/v1/Auth/connect";     // Triggered when an unauthenticated user hits a protected route
                        options.Events.OnRedirectToLogin = context =>
                        {
                            // When an unauthenticated user tries to access a protected route, it returns a 401 Unauthorized status instead of redirecting to the login page.
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        };
                        options.Events.OnRedirectToAccessDenied = context =>
                        {
                            // When a user is denied access due to insufficient permissions, it returns a 403 Forbidden status instead of redirecting to an access-denied page.
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        };
                    }
                )
                .AddOpenIdConnect(OpenIdScheme, options =>
                    {
                        options.Authority = authority;
                        options.ClientId = clientId;
                        options.ClientSecret = clientSecret;
                        options.ResponseType = OpenIdConnectResponseType.Code;
                        options.UsePkce = true;
                        options.SaveTokens = true;
                        options.Scope.Clear();
                        options.Scope.Add("openid");
                        options.Scope.Add("profile");
                        options.Scope.Add("offline_access");
                        options.CallbackPath = callbackPath;
                    });

        return services;
    }
}