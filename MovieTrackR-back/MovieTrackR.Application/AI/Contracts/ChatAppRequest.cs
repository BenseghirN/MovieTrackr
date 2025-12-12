using MovieTrackR.Domain.Entities.AI;

namespace MovieTrackR.Application.AI.Contracts;

public class ChatAppRequest
{
    public List<ChatMessage> Messages { get; set; }
    public string Format { get; set; } = string.Empty;
    public bool WebSearch { get; set; }
    public List<string>? Attachments { get; set; } // Gère les fichiers attachés, si besoin
    public string? Approach { get; set; } = string.Empty;
    public string? Context { get; set; } = string.Empty;
    public AgentContext? AgentContext { get; set; }

    public ChatAppRequest()
    {
        Messages = new List<ChatMessage>();
        Attachments = new List<string>();
    }
}

public class ChatMessage
{
    public string? Role { get; set; } // "user" ou "assistant"
    public string? Content { get; set; }
    public List<string> Attachments { get; set; } // Gère les fichiers attachés, si besoin

    public ChatMessage()
    {
        Attachments = new List<string>();
    }
}