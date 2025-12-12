using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieTrackR.Application.AI.Contracts;
using MovieTrackR.Application.AI.Queries;

namespace MovieTrackR.API.Endpoints.AI;

public static class AIHandlers
{
    public static async Task<IResult> Chat([FromBody] ChatAppRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        ChatResponse response = await mediator.Send(new ChatAppQuery(request), cancellationToken);
        return TypedResults.Ok(response);
    }
}
