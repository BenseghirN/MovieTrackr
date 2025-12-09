using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.Extensions.FileProviders;
using MovieTrackR.api.middleware;
using MovieTrackR.API.Configuration;
using MovieTrackR.API.Endpoints.Auth;
using MovieTrackR.API.Endpoints.Genres;
using MovieTrackR.API.Endpoints.Movies;
using MovieTrackR.API.Endpoints.People;
using MovieTrackR.API.Endpoints.ReviewComments;
using MovieTrackR.API.Endpoints.ReviewLikes;
using MovieTrackR.API.Endpoints.Reviews;
using MovieTrackR.API.Endpoints.UserLists;
using MovieTrackR.API.Endpoints.Users;
using MovieTrackR.Application.Configuration;
using MovieTrackR.Application.Interfaces;
using MovieTrackR.Infrastructure.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddSwaggerConfiguration()
    .AddCorsConfiguration()
    .AddApiVersioningConfiguration()
    .AddAuthorization()
    .AddRateLimitingConfiguration()
    .AddAzureAnthenticationConfiguration(builder.Configuration)
    .AddAppAuthorization() // custom Authorization policies
    .AddEndpointsApiExplorer()
    .AddInfrastructure(builder.Configuration) //custom service from Infrastructure project
    .AddApplication(builder.Configuration) //custom service from Application project
    .AddValidatorsFromAssembly(typeof(Program).Assembly) // Register FluentValidation validators
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .Services
    .AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("MovieTrackRDatabase")
        ?? throw new InvalidOperationException("Connection string 'MovieTrackRDatabase' not found."));

WebApplication app = builder.Build();

// Seed des genres au démarrage
using (var scope = app.Services.CreateScope())
{
    IGenreSeeder seeder = scope.ServiceProvider.GetRequiredService<IGenreSeeder>();
    await seeder.SeedGenresAsync();
}

// Swagger
if (app.Environment.IsDevelopment()) app.UseSwaggerConfiguration();

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app
.UseGlobalExceptionHandler()
.UseCorsConfiguration(app.Environment)
.UseAuthentication()
.UseAuthorization();
app.UseRateLimitingConfiguration();

// Map endpoints
app
.MapAuthEndpoints()
.MapMoviesEndpoints()
.MapReviewCommentsEndpoints()
.MapReviewLikesEndpoints()
.MapReviewsEndpoints()
.MapUserListsEndpoints()
.MapUsersEndpoints()
.MapGenresEndpoints()
.MapPeopleEndpoints()
.MapUserProfilesEndpoints()
.MapHealthChecks("/health");

// Sert index.html dans /browser comme page par défaut
app.ConfigureStaticFiles(builder.Environment);

app.Run();