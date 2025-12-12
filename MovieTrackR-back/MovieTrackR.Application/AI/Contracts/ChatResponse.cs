using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Domain.Entities.AI;

namespace MovieTrackR.Application.AI.Contracts;

public class ChatResponse
{
    public string Message { get; set; }
    public AuthorRole AuthorRole { get; set; }
    public object? Attachments { get; set; }
    public List<string>? WebSources { get; set; }
    public object? AdditionalContext { get; set; }

    public ChatResponse(string message, AuthorRole authorRole, object? attachments = null, object? webSources = null)
    {
        Message = message;
        AuthorRole = authorRole;
        Attachments = attachments;
        WebSources = webSources as List<string>;
    }

    public static ChatResponse BuildChatResponse(ChatHistory chatHistory, AgentContext agentContext)
    {
        ChatMessageContent LastMessage = chatHistory.Last();
        object? attachments = agentContext.TryGetValue("attachments", out var attach) ? attach : null;
        object? additionalContext = agentContext.TryGetValue("additionalContext", out var addCtx) ? addCtx : null;
        List<string> webSources = [.. agentContext.WebSources];
        return new ChatResponse(
            LastMessage.Content ?? throw new ArgumentNullException(nameof(LastMessage.Content), "Content of the last message cannot be null."),
            LastMessage.Role,
            attachments,
            webSources
        )
        {
            AdditionalContext = additionalContext
        };
    }
}