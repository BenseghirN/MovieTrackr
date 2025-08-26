using System.Reflection;
using Microsoft.OpenApi.Models;

namespace MovieTrackR.API.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.UseInlineDefinitionsForEnums();

            options.TagActionsBy(api =>
            {
                return new[] { api.GroupName ?? "MovieTrackR" };
            });

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MovieTrackR API",
                Version = "v1",
                Description = "API du projet MovieTrackR, Site de critiques de films collaboratif"
            });
        });

        return services;
    }

    public static void UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieTrackR API V1");
        });
    }
}
