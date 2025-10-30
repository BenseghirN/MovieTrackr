using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MovieTrackR.Application.Common.Services;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;

namespace MovieTrackR.Infrastructure.Configuration;

public static class TmdbDependencyInjection
{
    public static IServiceCollection AddTmdb(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<TmdbOptions>(config.GetSection(TmdbOptions.SectionName));

        services.AddHttpClient<ITmdbClient, TmdbHttpClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                TmdbOptions opt = sp.GetRequiredService<IOptions<TmdbOptions>>().Value;

                client.BaseAddress = new Uri(
                    string.IsNullOrWhiteSpace(opt.BaseUrl)
                        ? "https://api.themoviedb.org/3"
                        : opt.BaseUrl);

                client.Timeout = TimeSpan.FromSeconds(opt.HttpTimeoutSeconds <= 0 ? 3 : opt.HttpTimeoutSeconds);
            });
        services.AddScoped<ITmdbCatalogService, TmdbCatalogService>();

        return services;
    }
}