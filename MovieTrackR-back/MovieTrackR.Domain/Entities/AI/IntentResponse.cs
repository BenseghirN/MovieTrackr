using MovieTrackR.Domain.Enums.AI;

namespace MovieTrackR.Domain.Entities.AI;

public class IntentResponse
{
    public List<IntentProcessingStep> Intents { get; } = new();
    public string Message { get; }

    public IntentResponse(List<IntentProcessingStep> intents, string message)
    {
        Intents = intents ?? new List<IntentProcessingStep>();
        Message = message;
    }

    public string GetMessage() => Message;
    public List<IntentProcessingStep> GetIntents() => Intents;
}

public class IntentProcessingStep
{
    public IntentType IntentType { get; }
    public string? AdditionalContext { get; set; }  // Add some context for the next agant 

    public IntentProcessingStep(IntentType intentType, string? additionalContext = null)
    {
        IntentType = intentType;
        AdditionalContext = additionalContext;
    }
}