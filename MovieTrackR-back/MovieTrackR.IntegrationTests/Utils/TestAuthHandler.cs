using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MovieTrackR.IntegrationTests.Utils;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Claims qui matchent exactement ClaimsExtensions.cs
        var claims = new[]
        {
            new Claim("oid", "ext-1"),
            new Claim("email", "u@test.dev"),
            new Claim("name", "User Test"),
            new Claim("given_name", "User"),
            new Claim("family_name", "Test"),
            new Claim(ClaimTypes.Role, "User")
        };

        ClaimsIdentity identity = new ClaimsIdentity(claims, "Test");
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}