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
using MovieTrackR.AI.Interfaces;
using MovieTrackR.AI.Utils;
using MovieTrackR.AI.Agents.ActorSeekerAgent.Plugins;

namespace MovieTrackR.AI.Agents.DiscoverMoviesAgent;

public sealed class DiscoverMovies(Kernel kernel, IMediator mediator) : IDiscoverMoviesAgent
{
    public async Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, IntentProcessingStep? intentStep = null, CancellationToken cancellationToken = default)
    {
        ChatCompletionAgent SimilarMovieSeeker = BuildAgent(intentStep?.IntentType ?? IntentType.SimilarMovieSeekerAgent);
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

        await foreach (ChatMessageContent response in SimilarMovieSeeker.InvokeAsync(agentChatHistory, cancellationToken: cancellationToken))
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

            if (data.Attachments is not null && data.Attachments.Count > 0)
                agentContext.Attachments = data.Attachments;
            else
                agentContext.Attachments = null;
        }
        catch (JsonException)
        {
            agentContext.Result = raw;
        }
    }

    private ChatCompletionAgent BuildAgent(IntentType intent = IntentType.DiscoverMovieAgent)
    {
        kernel.Plugins.AddFromObject(new DiscoverMoviesPlugin(mediator));
        return new ChatCompletionAgent
        {
            Name = DiscoverMoviesProperties.Name,
            Instructions = DiscoverMoviesProperties.GetInstructions(intent),
            Kernel = kernel,
            Description = DiscoverMoviesProperties.Description,
            Arguments = new KernelArguments(
                    new OpenAIPromptExecutionSettings()
                    {
                        ServiceId = AiOptions.KernelService,
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                        // MaxTokens = 900,
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

        [JsonPropertyName("attachments")]
        public List<DiscoverMovieCandidateAttachment>? Attachments { get; set; }
    }
}