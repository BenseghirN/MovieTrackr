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

namespace MovieTrackR.AI.Agents.ActorSeekerAgent;

public sealed class PersonSeeker(Kernel kernel, IMediator mediator) : IPersonSeekerAgent
{
    public async Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, IntentResponse? intentResponse = null, CancellationToken cancellationToken = default)
    {
        ChatCompletionAgent ActorSeekerAgent = BuildAgent();
        ChatHistory agentChatHistory = new ChatHistory();

        // 1) Reprendre l’historique (sans System si tu veux)
        foreach (var message in chatHistory.Where(m => m.Role != AuthorRole.System).TakeLast(6))
        {
            if (!string.IsNullOrWhiteSpace(message.Content))
                agentChatHistory.AddMessage(message.Role, message.Content!);
        }

        // 2) Injecter un contexte "router" (System)
        if (intentResponse is not null && !string.IsNullOrWhiteSpace(intentResponse.Message))
        {
            agentChatHistory.AddMessage(
                AuthorRole.System,
                $"Context for this step (from router): {intentResponse.Message}"
            );
        }

        // 3) Accumuler la réponse (évite le JSON cassé par le streaming)
        StringBuilder sb = new StringBuilder();

        await foreach (ChatMessageContent response in ActorSeekerAgent.InvokeAsync(agentChatHistory, cancellationToken: cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(response.Content)) continue;
            sb.Append(response.Content);
        }

        string raw = sb.ToString().Trim();
        // 4) Parser JSON si possible
        try
        {
            AgentResponseData? jsonData = JsonSerializer.Deserialize<AgentResponseData>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (!string.IsNullOrWhiteSpace(jsonData?.Message))
                agentContext.Result = jsonData.Message;
            else
                agentContext.Result = raw;

            if (!string.IsNullOrWhiteSpace(jsonData?.AdditionalContext))
                agentContext.AdditionalContext = jsonData.AdditionalContext;
        }
        catch
        {
            // fallback si l’agent n’a pas renvoyé du JSON
            agentContext.Result = raw;
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
                        ServiceId = "MovieTrackR",
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                        MaxTokens = 200,
                        Temperature = 0.2,
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