using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieTrackR.Application.AI.Interfaces;
using MovieTrackR.AI.Agents.IntentExtractorAgent;
using MovieTrackR.AI.Agents.RedactorAgent;
using MovieTrackR.AI.Agents.RouteurAgent;
using MovieTrackR.AI.Builder;
using MovieTrackR.AI.Interfaces;
using MovieTrackR.AI.Utils;

namespace MovieTrackR.AI.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddAI(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AiOptions>(config.GetSection(AiOptions.SectionName));

        // Enregistrement de la configuration du Kernel
        services.AddScoped<SemanticKernelBuilder>();

        services.AddScoped<IRouteurAgent, Routeur>();
        services.AddScoped<IntentExtractor>();

        //Enregistrement des différents agents IA & Plugins
        services.AddScoped<IRedactorAgent, Redactor>();

        // services.AddSingleton<UserPlugin>();
        // services.AddSingleton<IUserAgent, UserAgent>();

        // services.AddSingleton<IIssAgent, IssAgent>();
        // services.AddSingleton<IBingAgent, BingAgent>();

        // services.AddSingleton<OcrPlugin>();
        // services.AddSingleton<IOcrAgent, OcrAgent>();

        return services;
    }
}
