namespace MovieTrackR.Domain.Entities.AI;

public sealed class AiChatResult
{
    public string Message { get; init; } = string.Empty;
    public object? AdditionalContext { get; init; }
    public List<string>? WebSources { get; init; }
}