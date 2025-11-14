using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTrackR.Application.Common.Exceptions;
using Npgsql;

namespace MovieTrackR.api.middleware;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errApp =>
        {
            errApp.Run(async ctx =>
            {
                IExceptionHandlerPathFeature? feature = ctx.Features.Get<IExceptionHandlerPathFeature>();
                Exception? ex = feature?.Error;

                ctx.Response.ContentType = "application/problem+json";
                ctx.Response.StatusCode = ex switch
                {
                    ValidationException => StatusCodes.Status400BadRequest,
                    InvalidOperationException => StatusCodes.Status400BadRequest,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    NotFoundException => StatusCodes.Status404NotFound,
                    ConflictException => StatusCodes.Status409Conflict,
                    ForbiddenException => StatusCodes.Status403Forbidden,
                    _ when IsConflict(ex) => StatusCodes.Status409Conflict,
                    OperationCanceledException => StatusCodes.Status499ClientClosedRequest,
                    _ => StatusCodes.Status500InternalServerError
                };

                object problem = CreateProblem(ctx, ex);
                await ctx.Response.WriteAsJsonAsync(problem);
            });
        });

        return app;
    }

    private static ProblemDetails CreateProblem(HttpContext ctx, Exception? ex)
    {
        int status = ctx.Response.StatusCode;
        ProblemDetails detail = ex is ValidationException vex
            ? new ValidationProblemDetails(
                  vex.Errors.GroupBy(e => e.PropertyName ?? string.Empty)
                            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            : new ProblemDetails { Detail = Sanitize(ex?.Message) };

        detail.Type = "https://tools.ietf.org/html/rfc7807";
        detail.Title = GetTitleFor(ex);
        detail.Status = status;
        detail.Extensions["traceId"] = ctx.TraceIdentifier;
        return detail;
    }

    private static string GetTitleFor(Exception? ex) => ex switch
    {
        KeyNotFoundException => "Not Found",
        UnauthorizedAccessException => "Unauthorized",
        InvalidOperationException => "Invalid Operation",
        ForbiddenException => "Forbidden",
        _ when IsConflict(ex) => "Conflict",
        _ => "Server Error"
    };

    private static string? Sanitize(string? message)
        => string.IsNullOrWhiteSpace(message) ? null : message;

    private static bool IsConflict(Exception? ex) =>
        ex is DbUpdateException db &&
        db.InnerException is PostgresException pg &&
        pg.SqlState == PostgresErrorCodes.UniqueViolation;
}