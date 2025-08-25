using System.Text.Json.Serialization;
using GameShelf.API.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddSwaggerConfiguration();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddAuthorization();
builder.Services.AddAzureAnthenticationConfiguration(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

WebApplication app = builder.Build();

app.MapGet("/hello", () => Results.Ok("Hello MovieTrackR!"))
   .WithName("Hello")
   .WithTags("Hello")
   .WithOpenApi();

// HTTPS Redirection
app.UseHttpsRedirection();

// Authentication
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}

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
