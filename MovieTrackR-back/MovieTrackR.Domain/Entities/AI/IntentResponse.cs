using MovieTrackR.Domain.Enums.AI;

namespace MovieTrackR.Domain.Entities.AI;

public class IntentResponse
{
    public List<IntentProcessingStep> Intents { get; init; } = new();
    public string Message { get; set; } = string.Empty;

    public static IntentResponse BuildIntent(List<IntentProcessingStep> intents, string message)
    {
        return new IntentResponse
        {
            Intents = intents,
            Message = message
        };
    }

    public string GetMessage() => Message;
    public List<IntentProcessingStep> GetIntents() => Intents;
}

public class IntentProcessingStep
{
    public IntentType IntentType { get; set; }
    public string? AdditionalContext { get; set; }  // Add some context for the next agant 

    public static IntentProcessingStep BuildProcessingSteps(IntentType intentType, string? context)
    {
        return new IntentProcessingStep
        {
            IntentType = intentType,
            AdditionalContext = context
        };
    }
}