using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MovieTrackR.Application.Common.Services;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Application.TMDB;
using MovieTrackR.Application.TMDB.Interfaces;
using MovieTrackR.Infrastructure.TMDB.Services;

namespace MovieTrackR.Infrastructure.Configuration;

public static class TmdbDependencyInjection
{
    public static IServiceCollection AddTmdb(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<TmdbOptions>(config.GetSection(TmdbOptions.SectionName));

        services.AddHttpClient<ITmdbClientService, TmdbHttpClientService>()
            .ConfigureHttpClient((sp, client) =>
            {
                var opt = sp.GetRequiredService<IOptions<TmdbOptions>>().Value;

                var baseUrl = string.IsNullOrWhiteSpace(opt.BaseUrl)
                    ? "https://api.themoviedb.org/3"
                    : opt.BaseUrl;
                if (!baseUrl.EndsWith("/")) baseUrl += "/";

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(opt.HttpTimeoutSeconds <= 0 ? 5 : opt.HttpTimeoutSeconds);

                if (!string.IsNullOrWhiteSpace(opt.AccessTokenV4))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", opt.AccessTokenV4);
                }
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });
        services.AddScoped<ITmdbCatalogService, TmdbCatalogService>();
        services.AddScoped<IGenreSeeder, GenreSeeder>();

        return services;
    }
}