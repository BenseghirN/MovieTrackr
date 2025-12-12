using MediatR;
using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Application.AI.Contracts;
using MovieTrackR.Application.AI.Interfaces;
using MovieTrackR.Domain.Entities.AI;

namespace MovieTrackR.Application.AI.Queries;

public sealed record ChatAppQuery(ChatAppRequest Request) : IRequest<ChatResponse>;

public sealed class ChatAppHandler(IRouteurAgent routeurAgent) : IRequestHandler<ChatAppQuery, ChatResponse>
{
    public async Task<ChatResponse> Handle(ChatAppQuery query, CancellationToken cancellationToken)
    {
        ChatHistory chatHistory = new();

        foreach (ChatMessage msg in query.Request.Messages)
        {
            if (string.IsNullOrWhiteSpace(msg.Content)) continue;

            AuthorRole role = msg.Role?.ToLowerInvariant() switch
            {
                "user" => AuthorRole.User,
                "assistant" => AuthorRole.Assistant,
                "system" => AuthorRole.System,
                _ => AuthorRole.User
            };

            chatHistory.AddMessage(role, msg.Content);
        }

        AgentContext agentContext = query.Request.AgentContext ?? new AgentContext();
        agentContext.Format = query.Request.Format;
        agentContext.WebSearch = query.Request.WebSearch;

        await routeurAgent.ProcessRequestAsync(chatHistory, agentContext, cancellationToken);

        chatHistory.AddAssistantMessage(agentContext.Result);

        return ChatResponse.BuildChatResponse(chatHistory, agentContext);
    }
}