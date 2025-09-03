using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace MovieTrackR.API.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // XML Docs from API Project
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string apiXml = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(apiXml))
                options.IncludeXmlComments(apiXml);
            // XML Docs from Application Project
            Assembly appAssembly = typeof(Application.DTOs.UserDto).Assembly;
            string appXml = Path.Combine(AppContext.BaseDirectory, $"{appAssembly.GetName().Name}.xml");
            if (File.Exists(appXml))
                options.IncludeXmlComments(appXml);

            options.UseInlineDefinitionsForEnums();
            options.SupportNonNullableReferenceTypes();

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
        IApiVersionDescriptionProvider provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
        app.UseSwaggerUI(options =>
        {
            foreach (ApiVersionDescription desc in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
                    $"MovieTrackR {desc.GroupName.ToUpperInvariant()}");
            }
            // options.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieTrackR API V1");
        });
    }
}
