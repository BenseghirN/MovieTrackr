using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieTrackR.Application.AI.Interfaces;
using MovieTrackR.AI.Agents.IntentExtractorAgent;
using MovieTrackR.AI.Agents.RedactorAgent;
using MovieTrackR.AI.Agents.RouteurAgent;
using MovieTrackR.AI.Builder;
using MovieTrackR.AI.Interfaces;
using MovieTrackR.AI.Utils;
using MovieTrackR.AI.Agents.ActorSeekerAgent;
using Microsoft.Extensions.Options;
using MovieTrackR.AI.Agents.SimilarMovieSeekerAgent;
using MovieTrackR.AI.Agents.DiscoverMoviesAgent;

namespace MovieTrackR.AI.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddAI(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AiOptions>(config.GetSection(AiOptions.SectionName));

        // Enregistrement de la configuration du Kernel
        services.AddScoped<SemanticKernelBuilder>();
        services.AddScoped(sp =>
        {
            SemanticKernelBuilder builder = sp.GetRequiredService<SemanticKernelBuilder>();
            AiOptions options = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            return builder.BuildKernel();
        });

        services.AddScoped<IRouteurAgent, Routeur>();
        services.AddScoped<IntentExtractor>();

        //Enregistrement des différents agents IA & Plugins
        services.AddScoped<IRedactorAgent, Redactor>();
        services.AddScoped<IPersonSeekerAgent, PersonSeeker>();
        services.AddScoped<ISimilarMovieSeekerAgent, SimilarMovieSeeker>();
        services.AddScoped<IDiscoverMoviesAgent, DiscoverMovies>();



        return services;
    }
}
