using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Domain.Entities.AI;

namespace MovieTrackR.Application.AI.Contracts;

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public AuthorRole AuthorRole { get; set; }
    public string? AdditionalContext { get; set; }

    // public object? Attachments { get; set; }

    public static ChatResponse BuildChatResponse(ChatHistory chatHistory, AgentContext agentContext)
    {
        ChatMessageContent LastMessage = chatHistory.Last();
        if (LastMessage.Content is null)
            throw new ArgumentNullException(nameof(LastMessage.Content), "Content of the last message cannot be null.");
        // object? attachments = agentContext.TryGetValue("attachments", out var attach) ? attach : null;

        return new ChatResponse
        {
            Message = LastMessage.Content,
            AuthorRole = LastMessage.Role,
            AdditionalContext = agentContext.AdditionalContext
        };
    }
}