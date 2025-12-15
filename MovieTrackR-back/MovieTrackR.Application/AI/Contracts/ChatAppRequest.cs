using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Domain.Entities.AI;

namespace MovieTrackR.Application.AI.Contracts;

public class ChatAppRequest
{
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public AgentContext? AgentContext { get; set; }

}

public class ChatMessage
{
    public string? Role { get; set; } = AuthorRole.User.ToString(); // "user" ou "assistant"
    public string? Content { get; set; }
    // public List<string> Attachments { get; set; } = new List<string>(); // Gère les fichiers attachés, si besoin
}