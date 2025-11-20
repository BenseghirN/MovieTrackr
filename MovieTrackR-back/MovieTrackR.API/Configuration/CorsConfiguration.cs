namespace MovieTrackR.API.Configuration;

public static class CorsConfiguration
{
    public const string DevelopmentPolicy = "DevelopmentPolicy";
    public const string ProductionPolicy = "ProductionPolicy";

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DevelopmentPolicy, policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });

            options.AddPolicy(ProductionPolicy, policy =>
            {
                policy.WithOrigins(
                        "https://app-movietrackr-dev-swecentral-001-fddwg2cecscwa7d0.swedencentral-01.azurewebsites.net"
                      )
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseCors(DevelopmentPolicy);
        }
        else
        {
            app.UseCors(ProductionPolicy);
        }

        return app;
    }
}