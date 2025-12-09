using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Infrastructure.Persistence;

namespace MovieTrackR.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext to PostgreSQL
        services.AddDbContextPool<MovieTrackRDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("MovieTrackRDatabase"),
                b =>
                {
                    b.MigrationsAssembly(typeof(MovieTrackRDbContext).Assembly.FullName);
                    b.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(2),
                        errorCodesToAdd: null
                    );
                }
            ).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IMovieTrackRDbContext>(provider => provider.GetRequiredService<MovieTrackRDbContext>());

        // TMDB Configuration
        services.AddTmdb(configuration);

        // Azure Storage for Avatars
        services.AddAzureStorage(configuration);

        return services;
    }
}