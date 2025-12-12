// #pragma warning disable SKEXP0001 , SKEXP0110
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MovieTrackR.Domain.Entities.IA;
using MovieTrackR.IA.Builder;
using MovieTrackR.IA.Interfaces;

namespace MovieTrackR.IA.Agents.RedactorAgent;

public sealed class Redactor(SemanticKernelBuilder builder) : IRedactorAgent
{
    private readonly Kernel _kernel =
        builder.BuildKernel(serviceId: "RedactorAgent");

    private ChatCompletionAgent BuildAgent(string? input = null, string? format = null)
    {
        return new ChatCompletionAgent
        {
            Name = RedactorProperties.Name,
            Instructions = RedactorProperties.GetInstructions(input!, format!),
            Kernel = _kernel,
            Description = RedactorProperties.Description,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings()
                {
                    ServiceId = "RedactorAgent",
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    Temperature = 0.3
                }
            )
        };
    }

    public async Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, IntentResponse? intentResponse = null)
    {
        ChatCompletionAgent RedactorAgent;
        if (agentContext.ContainsKey("combinedResponses") && agentContext["combinedResponses"] is string combinedResponses)
        {
            if (agentContext.ContainsKey("format") && agentContext["format"] is string format)
            {
                RedactorAgent = BuildAgent(combinedResponses, format);
            }
            else
                RedactorAgent = BuildAgent(combinedResponses);
        }
        else
        {
            Console.WriteLine("❌ Aucun combinedResponses détecté ou format incorrect.");
            RedactorAgent = BuildAgent();
        }

        ChatHistory agentChatHistory = new ChatHistory();
        foreach (ChatMessageContent message in chatHistory)
        {
            if (message.Role != AuthorRole.System)
            {
                agentChatHistory.AddMessage(message.Role, message.Content!);
            }
        }

        await foreach (ChatMessageContent response in RedactorAgent.InvokeAsync(agentChatHistory))
        {
            if (!string.IsNullOrWhiteSpace(response.Content))
            {
                agentContext.Result = response.Content;
            }
        }
    }

    private class AgentResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
        [JsonPropertyName("webSources")]
        public List<string> WebSources { get; set; } = new List<string>();
    }
}
// #pragma warning restore SKEXP0001, SKEXP0110