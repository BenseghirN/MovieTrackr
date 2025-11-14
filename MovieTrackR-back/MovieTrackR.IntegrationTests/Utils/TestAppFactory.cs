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

namespace MovieTrackR.IntegrationTests.Utils;

public sealed class TestAppFactory : WebApplicationFactory<Program>
{
    private readonly PostgresFixture _pg;
    public Mock<ITmdbClient> TmdbMock { get; } = new(MockBehavior.Strict);

    public TestAppFactory(PostgresFixture pg) => _pg = pg;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["EntraExternalId:Authority"] = "https://fake-authority.test",
                ["EntraExternalId:ClientId"] = "fake-client-id",
                ["EntraExternalId:ClientSecret"] = "fake-secret",
                ["EntraExternalId:CallbackPath"] = "/signin-oidc",
                ["EntraExternalId:OpenIdScheme"] = "Test"
            }!);
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

            services.RemoveAll<ITmdbClient>();
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

        IHost host = base.CreateHost(builder);

        return host;
    }

    public HttpClient CreateClientAuthenticated()
        => WithWebHostBuilder(_ => { })
           .CreateClient(new WebApplicationFactoryClientOptions
           {
               AllowAutoRedirect = false
           });
}