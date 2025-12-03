using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.Extensions.FileProviders;
using MovieTrackR.api.middleware;
using MovieTrackR.API.Configuration;
using MovieTrackR.API.Endpoints.Auth;
using MovieTrackR.API.Endpoints.Genres;
using MovieTrackR.API.Endpoints.Movies;
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
    });

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
app.UseGlobalExceptionHandler();

app.UseCorsConfiguration(app.Environment);
app.UseAuthentication();
app.UseAuthorization();
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
.MapGenresEndpoints();

// Sert index.html dans /browser comme page par défaut
string browserRoot = Path.Combine(builder.Environment.WebRootPath, "browser");
if (Directory.Exists(browserRoot))
{
    PhysicalFileProvider browserFileProvider = new PhysicalFileProvider(browserRoot);

    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = browserFileProvider,
        RequestPath = ""
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = browserFileProvider,
        RequestPath = ""
    });

    app.MapFallback(async context =>
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(browserRoot, "index.html"));
    });
}
else Console.WriteLine("⚠️ Angular app not found in wwwroot/browser. Run 'npm run build' in the frontend project.");

// Sert tous les fichiers statiques depuis /wwwroot/browser
// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = new PhysicalFileProvider(
//         Path.Combine(builder.Environment.WebRootPath, "browser")),
//     RequestPath = "",
//     OnPrepareResponse = ctx =>
//     {
//         var path = ctx.File.PhysicalPath ?? string.Empty;
//         if (path.EndsWith(".json") || path.EndsWith(".js") || path.EndsWith(".css"))
//         {
//             ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
//             ctx.Context.Response.Headers["Pragma"] = "no-cache";
//             ctx.Context.Response.Headers["Expires"] = "0";
//         }
//     }
// });

// Fallback SPA : toutes les routes non trouvées renvoient browser/index.html
// app.MapFallback(async context =>
// {
//     context.Response.ContentType = "text/html";
//     await context.Response.SendFileAsync(Path.Combine(builder.Environment.WebRootPath, "browser", "index.html"));
// });

app.Run();
