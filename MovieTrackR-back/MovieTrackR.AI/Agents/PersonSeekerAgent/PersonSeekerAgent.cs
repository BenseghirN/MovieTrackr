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

        try
        {
            AgentResponseData? data = JsonSerializer.Deserialize<AgentResponseData>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (data is null)
            {
                agentContext.Result = raw;
                return;
            }

            agentContext.Result = !string.IsNullOrWhiteSpace(data.Message) ? data.Message : raw;

            if (!string.IsNullOrWhiteSpace(data.AdditionalContext))
                agentContext.AdditionalContext = data.AdditionalContext;
        }
        catch (JsonException)
        {
            agentContext.Result = raw;
        }
    }

    private ChatCompletionAgent BuildAgent(IntentType intent = IntentType.PersonSeekerAgent)
    {
        kernel.Plugins.AddFromObject(new PersonSeekerPlugin(mediator));
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