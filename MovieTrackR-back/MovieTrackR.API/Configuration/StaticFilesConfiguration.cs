using Microsoft.Extensions.FileProviders;

namespace MovieTrackR.API.Configuration;

public static class StaticFilesConfiguration
{
    public static void ConfigureStaticFiles(this WebApplication app, IWebHostEnvironment environment)
    {
        if (string.IsNullOrEmpty(environment.WebRootPath))
        {
            Console.WriteLine("⚠️ wwwroot folder not found. Skipping static files configuration.");
            return;
        }

        string browserRoot = Path.Combine(environment.WebRootPath, "browser");

        if (!Directory.Exists(browserRoot))
        {
            Console.WriteLine("⚠️ Angular app not found in wwwroot/browser. Run 'npm run build' in the frontend project.");
            return;
        }

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
}