using MediatR;
using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Application.AI.Contracts;
using MovieTrackR.Application.AI.Interfaces;
using MovieTrackR.Domain.Entities.AI;

namespace MovieTrackR.Application.AI.Queries;

public sealed record ChatAppQuery(ChatAppRequest Request, ChatHistory ChatHistory) : IRequest<ChatResponse>;

public sealed class ChatAppHandler(IRouteur routeurAgent) : IRequestHandler<ChatAppQuery, ChatResponse>
{
    public async Task<ChatResponse> Handle(ChatAppQuery query, CancellationToken cancellationToken)
    {
        ChatHistory chatHistory = query.ChatHistory;

        AgentContext agentContext = query.Request.AgentContext ?? new AgentContext();

        await routeurAgent.ProcessRequestAsync(chatHistory, agentContext, cancellationToken);

        return ChatResponse.BuildChatResponse(chatHistory, agentContext);
    }
}