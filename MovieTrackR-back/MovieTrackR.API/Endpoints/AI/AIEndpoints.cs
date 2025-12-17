using Asp.Versioning;
using Asp.Versioning.Builder;
using MovieTrackR.Application.AI.Contracts;

namespace MovieTrackR.API.Endpoints.AI;

public static class AIEndpoints
{
    public static IEndpointRouteBuilder MapAIEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet vset = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/ai")
            .WithApiVersionSet(vset)
            .MapToApiVersion(1, 0)
            .WithTags("AI")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/chat", AIHandlers.Chat)
            .WithName("Chat_AI")
            .WithSummary("Chat with MovieTrackR AI")
            .Produces<ChatResponse>(StatusCodes.Status200OK);

        group.MapDelete("/chat/resetSession", AIHandlers.ResetChatHistory)
            .WithName("Reset_Chat_Session")
            .WithSummary("Reset the user session for chat history")
            .Produces(StatusCodes.Status200OK);

        return app;
    }
}