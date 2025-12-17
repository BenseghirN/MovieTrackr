using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MovieTrackR.Domain.Entities.AI;
using MovieTrackR.AI.Builder;
using MovieTrackR.AI.Interfaces;
using MovieTrackR.AI.Utils;
using MovieTrackR.Domain.Enums.AI;
using System.Text;

namespace MovieTrackR.AI.Agents.RedactorAgent;

public sealed class Redactor(Kernel kernel) : IRedactorAgent
{
    private ChatCompletionAgent BuildAgent(IntentType intent = IntentType.ReviewRedactorAgent)
    {
        return new ChatCompletionAgent
        {
            Name = RedactorProperties.Name,
            Instructions = RedactorProperties.GetInstructions(intent),
            Kernel = kernel,
            Description = RedactorProperties.Description,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings()
                {
                    ServiceId = AiOptions.KernelService,
                    Temperature = 0.2,
                    // FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                }
            )
        };
    }

    public async Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, IntentProcessingStep? intentStep = null, CancellationToken cancellationToken = default)
    {
        ChatCompletionAgent RedactorAgent = BuildAgent(intentStep?.IntentType ?? IntentType.ReviewRedactorAgent);

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

        await foreach (ChatMessageContent response in RedactorAgent.InvokeAsync(agentChatHistory, cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(response.Content))
                sb.Append(response.Content);
        }

        string result = sb.ToString().Trim();

        agentContext.Result = !string.IsNullOrWhiteSpace(result)
            ? result
            : "Désolé, je n'ai pas pu reformuler la critique.";
    }
}