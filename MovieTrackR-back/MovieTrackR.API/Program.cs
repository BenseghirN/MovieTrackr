using System.Text.Json.Serialization;
using FluentValidation;
using MovieTrackR.api.middleware;
using MovieTrackR.API.Configuration;
using MovieTrackR.API.Endpoints.Auth;
using MovieTrackR.API.Endpoints.Movies;
using MovieTrackR.API.Endpoints.Users;
using MovieTrackR.Application.Configuration;
using MovieTrackR.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddSwaggerConfiguration()
    .AddApiVersioningConfiguration()
    .AddAuthorization()
    .AddAzureAnthenticationConfiguration(builder.Configuration)
    .AddAppAuthorization() // custom Authorization policies
    .AddEndpointsApiExplorer()
    .AddInfrastructure(builder.Configuration) //custom service from Infrastructure project
    .AddApplication() //custom service from Application project
    .AddValidatorsFromAssembly(typeof(Program).Assembly) // Register FluentValidation validators
    .AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

WebApplication app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment()) app.UseSwaggerConfiguration();

app.UseHttpsRedirection();
app.UseGlobalExceptionHandler();

// Map endpoints
app
.MapAuthEndpoints()
.MapUsersEndpoints()
.MapMoviesEndpoints();

app.UseAuthentication();
app.UseAuthorization();


// Static files server for static frontend
app.UseDefaultFiles(); // Automaticaly serve index.html if exists
app.UseStaticFiles(new StaticFileOptions // Serve  static files (CSS, JS, images, etc.)
{
    OnPrepareResponse = ctx =>
    {
        string path = ctx.File.PhysicalPath ?? string.Empty;
        if (path.EndsWith(".json") || path.EndsWith(".js") || path.EndsWith(".css"))
        {
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
            ctx.Context.Response.Headers["Expires"] = "0";
        }
    }
});

app.Run();
