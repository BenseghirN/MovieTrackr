using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace MovieTrackR.api.middleware;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errApp =>
        {
            errApp.Run(async ctx =>
            {
                var feature = ctx.Features.Get<IExceptionHandlerPathFeature>();
                var ex = feature?.Error;

                ctx.Response.ContentType = "application/problem+json";
                ctx.Response.StatusCode = ex switch
                {
                    ValidationException => StatusCodes.Status400BadRequest,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    InvalidOperationException => StatusCodes.Status400BadRequest,
                    OperationCanceledException => StatusCodes.Status499ClientClosedRequest, // si supporté
                    _ when IsConflict(ex) => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError
                };

                var problem = CreateProblem(ctx, ex);
                await ctx.Response.WriteAsJsonAsync(problem);
            });
        });

        return app;
    }

    private static object CreateProblem(HttpContext ctx, Exception? ex)
    {
        var traceId = ctx.TraceIdentifier;

        if (ex is ValidationException vex)
        {
            var errors = vex.Errors
                .GroupBy(e => e.PropertyName ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Validation failed",
                status = StatusCodes.Status400BadRequest,
                traceId,
                errors
            };
        }

        // payload générique RFC7807
        return new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title = GetTitleFor(ex),
            status = ctx.Response.StatusCode,
            detail = Sanitize(ex?.Message),
            traceId
        };
    }

    private static string GetTitleFor(Exception? ex) => ex switch
    {
        KeyNotFoundException => "Not Found",
        UnauthorizedAccessException => "Unauthorized",
        InvalidOperationException => "Invalid Operation",
        _ when IsConflict(ex) => "Conflict",
        _ => "Server Error"
    };

    private static string? Sanitize(string? message)
        => string.IsNullOrWhiteSpace(message) ? null : message;

    // Détecte un conflit (ex: UNIQUE violation PostgreSQL / EF Core)
    private static bool IsConflict(Exception? ex)
        => ex is Microsoft.EntityFrameworkCore.DbUpdateException db &&
           (db.InnerException?.Message?.Contains("unique", StringComparison.OrdinalIgnoreCase) ?? false);
}