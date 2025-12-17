using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MovieTrackR.AI.Utils;
using MovieTrackR.Domain.Entities.AI;
using MovieTrackR.Domain.Enums.AI;

namespace MovieTrackR.AI.Agents.IntentExtractorAgent;

public sealed class IntentExtractor(Kernel kernel)
{
    private ChatCompletionAgent BuildAgent()
    {
        return new ChatCompletionAgent
        {
            Name = IntentExtractorAgentProperties.Name,
            Description = IntentExtractorAgentProperties.Description,
            Instructions = IntentExtractorAgentProperties.Instructions,
            Kernel = kernel,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings()
                {
                    ServiceId = AiOptions.KernelService
                }
            )
        };
    }

    public async Task<IntentResponse> ExtractIntent(ChatHistory chatHistory, AgentContext context, CancellationToken cancellationToken)
    {
        ChatHistory agentChatHistory = new ChatHistory();

        if (!string.IsNullOrWhiteSpace(context?.AdditionalContext))
            agentChatHistory.AddSystemMessage($"Current selected context (do not guess, use as truth): {context.AdditionalContext}");

        foreach (ChatMessageContent message in chatHistory.Where(m => m.Role != AuthorRole.System).TakeLast(6))
        {
            if (!string.IsNullOrWhiteSpace(message.Content))
                agentChatHistory.AddMessage(message.Role, message.Content);
        }

        ChatCompletionAgent intentExtractorAgent = BuildAgent();
        StringBuilder sb = new StringBuilder();
        await foreach (ChatMessageContent response in intentExtractorAgent.InvokeAsync(agentChatHistory, cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(response.Content))
                sb.Append(response.Content);
        }

        string raw = sb.ToString().Trim();

        try
        {
            IntentResponseData? jsonData = JsonSerializer.Deserialize<IntentResponseData>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (jsonData?.Intents?.Any() == true)
            {
                List<IntentProcessingStep> intentSteps = jsonData.Intents.Select(intent =>
                    IntentProcessingStep.BuildProcessingSteps(
                        Enum.TryParse(intent.IntentType, true, out IntentType parsedIntent) ? parsedIntent : IntentType.None,
                        intent.AdditionalContext
                    )).ToList();

                return IntentResponse.BuildIntent(intentSteps, jsonData.ClarifySentence ?? "");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error parsing intent JSON: {ex.Message}");
            return IntentResponse.BuildIntent(new List<IntentProcessingStep>(), "❌ Unable to determine intent.");
        }

        return IntentResponse.BuildIntent(new List<IntentProcessingStep>(), "❌ No response from intent agent.");
    }

    private class IntentResponseData
    {
        [JsonPropertyName("intents")]
        public List<IntentData> Intents { get; set; } = new();

        [JsonPropertyName("clarify_sentence")]
        public string? ClarifySentence { get; set; }
    }

    private class IntentData
    {
        [JsonPropertyName("intent_type")]
        public string? IntentType { get; set; }

        [JsonPropertyName("additional_context")]
        public string? AdditionalContext { get; set; }
    }
}