using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MovieTrackR.Domain.Entities.AI;
using MovieTrackR.Domain.Enums.AI;
using MovieTrackR.AI.Agents.ActorSeekerAgent.Plugins;
using MovieTrackR.AI.Interfaces;
using MovieTrackR.AI.Utils;

namespace MovieTrackR.AI.Agents.ActorSeekerAgent;

public sealed class PersonSeeker(Kernel kernel, IMediator mediator) : IPersonSeekerAgent
{
    public async Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, IntentProcessingStep? intentStep = null, CancellationToken cancellationToken = default)
    {
        ChatCompletionAgent ActorSeekerAgent = BuildAgent(intentStep?.IntentType ?? IntentType.PersonSeekerAgent);
        ChatHistory agentChatHistory = new ChatHistory();

        foreach (ChatMessageContent message in chatHistory.Where(m => m.Role != AuthorRole.System).TakeLast(6))
        {
            if (!string.IsNullOrWhiteSpace(message.Content))
                agentChatHistory.AddMessage(message.Role, message.Content!);
        }

        if (!string.IsNullOrWhiteSpace(intentStep?.AdditionalContext))
        {
            agentChatHistory.AddSystemMessage(
                $"Current step instruction (follow strictly): {intentStep.AdditionalContext}"
            );
        }

        if (!string.IsNullOrWhiteSpace(agentContext.AdditionalContext))
        {
            agentChatHistory.AddSystemMessage(
                $"Selected context (DO NOT GUESS): {agentContext.AdditionalContext}"
            );
        }

        StringBuilder sb = new StringBuilder();

        await foreach (ChatMessageContent response in ActorSeekerAgent.InvokeAsync(agentChatHistory, cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(response.Content))
                sb.Append(response.Content);
        }

        string raw = sb.ToString().Trim();

        if (!LooksLikeJson(raw))
        {
            agentContext.Result = raw;
            return;
        }

        try
        {
            // Parse le JSON externe
            AgentResponseData? jsonData = JsonSerializer.Deserialize<AgentResponseData>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (jsonData is null)
            {
                agentContext.Result = raw;
                return;
            }

            // ✅ Gestion du double-encodage dans message
            if (LooksLikeJsonText(jsonData.Message))
            {
                var decodedMessage = TryDecodeJsonString(jsonData.Message);

                if (decodedMessage != null && LooksLikeJson(decodedMessage))
                {
                    try
                    {
                        // Parse le JSON interne
                        AgentResponseData? innerData = JsonSerializer.Deserialize<AgentResponseData>(
                            decodedMessage,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (innerData != null)
                        {
                            // Remplace message ET additional_context
                            if (!string.IsNullOrWhiteSpace(innerData.Message))
                                jsonData.Message = innerData.Message;

                            if (!string.IsNullOrWhiteSpace(innerData.AdditionalContext))
                                jsonData.AdditionalContext = innerData.AdditionalContext;
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"⚠️ Failed to parse inner JSON: {ex.Message}");
                        // Garde le message décodé brut
                        jsonData.Message = decodedMessage;
                    }
                }
            }

            // ✅ Gestion du double-encodage dans additional_context
            if (LooksLikeJsonText(jsonData.AdditionalContext))
            {
                var decoded = TryDecodeJsonString(jsonData.AdditionalContext);
                if (!string.IsNullOrWhiteSpace(decoded))
                    jsonData.AdditionalContext = decoded;
            }

            agentContext.Result = !string.IsNullOrWhiteSpace(jsonData.Message)
                ? jsonData.Message
                : raw;

            if (!string.IsNullOrWhiteSpace(jsonData.AdditionalContext))
                agentContext.AdditionalContext = jsonData.AdditionalContext;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"❌ Invalid JSON from agent: {ex.Message}\nRAW: {raw}");
            agentContext.Result = raw;
        }
    }

    private static bool LooksLikeJson(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        return (s.StartsWith("{") && s.EndsWith("}"))
            || (s.StartsWith("[") && s.EndsWith("]"));
    }

    private static bool LooksLikeJsonText(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();

        // Détecte les strings JSON encodées ou les objets JSON
        return (s.StartsWith("\"{") && s.EndsWith("}\""))
            || (s.StartsWith("{") && s.EndsWith("}"))
            || (s.StartsWith("\"[") && s.EndsWith("]\""))
            || (s.StartsWith("[") && s.EndsWith("]"));
    }
    private static string? TryDecodeJsonString(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;

        s = s.Trim();

        // Si c'est déjà un objet JSON brut, on le retourne tel quel
        if ((s.StartsWith("{") && s.EndsWith("}"))
         || (s.StartsWith("[") && s.EndsWith("]")))
        {
            return s;
        }

        // Sinon on essaie de désérialiser comme une string JSON
        try
        {
            return JsonSerializer.Deserialize<string>(s);
        }
        catch
        {
            return null;
        }
    }

    private ChatCompletionAgent BuildAgent(IntentType intent = IntentType.PersonSeekerAgent)
    {
        kernel.Plugins.AddFromObject(new ActorSeekerPlugin(mediator));
        return new ChatCompletionAgent
        {
            Name = PersonSeekerProperties.Name,
            Instructions = PersonSeekerProperties.GetInstructions(intent),
            Kernel = kernel,
            Description = PersonSeekerProperties.Description,
            Arguments = new KernelArguments(
                    new OpenAIPromptExecutionSettings()
                    {
                        ServiceId = AiOptions.KernelService,
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                        MaxTokens = 800,
                        Temperature = 0.2,
                        ResponseFormat = "json_object"
                    }
                )
        };
    }

    private class AgentResponseData
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("additional_context")]
        public string? AdditionalContext { get; set; }
    }
}