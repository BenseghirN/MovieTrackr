using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieTrackR.Application.IA.Interfaces;
using MovieTrackR.IA.Agents.IntentExtractorAgent;
using MovieTrackR.IA.Agents.RedactorAgent;
using MovieTrackR.IA.Agents.RouteurAgent;
using MovieTrackR.IA.Builder;
using MovieTrackR.IA.Interfaces;
using MovieTrackR.IA.Utils;

namespace MovieTrackR.IA.Configuration;

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
