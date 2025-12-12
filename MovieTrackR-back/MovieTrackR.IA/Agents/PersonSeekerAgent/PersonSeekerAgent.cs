using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MovieTrackR.Domain.Entities.IA;
using MovieTrackR.Domain.Enums.IA;
using MovieTrackR.IA.Agents.ActorSeekerAgent.Plugins;
using MovieTrackR.IA.Builder;
using MovieTrackR.IA.Interfaces;

namespace MovieTrackR.IA.Agents.ActorSeekerAgent;

public sealed class PersonSeeker(SemanticKernelBuilder builder, IMediator mediator, CancellationToken cancellationToken = default) : IPersonSeekerAgent
{
    private readonly Kernel _kernel = CreateKernel(builder, mediator);

    private ChatCompletionAgent BuildAgent(IntentType intent = IntentType.PersonSeekerAgent)
    {
        return new ChatCompletionAgent
        {
            Name = PersonSeekerProperties.Name,
            Instructions = PersonSeekerProperties.GetInstructions(intent),
            Kernel = _kernel,
            Description = PersonSeekerProperties.Description,
            Arguments = new KernelArguments(
                    new OpenAIPromptExecutionSettings()
                    {
                        ServiceId = PersonSeekerProperties.Service,
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    }
                )
        };
    }

    public async Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, IntentResponse? intentResponse = null)
    {
        ChatCompletionAgent ActorSeekerAgent = BuildAgent();
        ChatHistory agentChatHistory = new ChatHistory();

        // 1) Reprendre l’historique (sans System si tu veux)
        foreach (ChatMessageContent message in chatHistory)
        {
            if (message.Role == AuthorRole.System) continue;
            if (string.IsNullOrWhiteSpace(message.Content)) continue;

            agentChatHistory.AddMessage(message.Role, message.Content);
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

    private static Kernel CreateKernel(SemanticKernelBuilder builder, IMediator mediator)
    {
        Kernel kernel = builder.BuildKernel(serviceId: PersonSeekerProperties.Service);
        kernel.Plugins.AddFromObject(new ActorSeekerPlugin(mediator));
        return kernel;
    }

    private class AgentResponseData
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("additional_context")]
        public string? AdditionalContext { get; set; }
    }
}