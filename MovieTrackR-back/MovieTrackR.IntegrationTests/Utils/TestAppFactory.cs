using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Infrastructure.Persistence;
using MovieTrackR.IntegrationTests.Containers;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.API;
using Microsoft.Extensions.Configuration;
using MovieTrackR.Application.TMDB;
using Microsoft.AspNetCore.Hosting;

namespace MovieTrackR.IntegrationTests.Utils;

public sealed class TestAppFactory : WebApplicationFactory<Program>
{
    private readonly PostgresFixture _pg;
    public Mock<ITmdbClientService> TmdbMock { get; } = new(MockBehavior.Strict);

    public TestAppFactory(PostgresFixture pg)
    {
        _pg = pg;
        ConfigureDefaultMocks();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:MovieTrackRDatabase", _pg.ConnectionString);

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EntraExternalId:Authority"] = "https://fake-authority.test",
                ["EntraExternalId:ClientId"] = "fake-client-id",
                ["EntraExternalId:ClientSecret"] = "fake-secret",
                ["EntraExternalId:CallbackPath"] = "/signin-oidc",
                ["EntraExternalId:OpenIdScheme"] = "Test",
                ["ConnectionStrings:MovieTrackRDatabase"] = _pg.ConnectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            List<ServiceDescriptor> descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<MovieTrackRDbContext>))
                .ToList();
            foreach (ServiceDescriptor d in descriptors) services.Remove(d);

            services.AddDbContextPool<MovieTrackRDbContext>(opts =>
            {
                opts.UseNpgsql(_pg.ConnectionString)
                    .UseSnakeCaseNamingConvention();
            });
            services.AddScoped<IMovieTrackRDbContext>(sp => sp.GetRequiredService<MovieTrackRDbContext>());

            services.RemoveAll<ITmdbClientService>();
            services.AddSingleton(_ => TmdbMock.Object);

            List<ServiceDescriptor> authDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("Authentication") == true)
                .ToList();
            foreach (ServiceDescriptor d in authDescriptors) services.Remove(d);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Test";
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.AddAuthorization();
        });
    }

    private void ConfigureDefaultMocks()
    {
        TmdbMock
            .Setup(c => c.GetGenresAsync(
                "fr-FR",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TmdbGenresResponse(
                Genres: new List<TmdbGenre>
                {
                    new(Id: 878, Name: "Science-Fiction"),
                    new(Id: 12, Name: "Aventure"),
                    new(Id: 28, Name: "Action")
                }
            ));
    }

    public HttpClient CreateClientAuthenticated()
        => WithWebHostBuilder(_ => { })
           .CreateClient(new WebApplicationFactoryClientOptions
           {
               AllowAutoRedirect = false
           });
}