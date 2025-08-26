using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieTrackR.Application.Interfaces;

namespace MovieTrackR.Infrastructure.Persistence;

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
                    b.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null);
                }
            ).UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IMovieTrackRDbContext>(provider => provider.GetRequiredService<MovieTrackRDbContext>());

        return services;
    }
}