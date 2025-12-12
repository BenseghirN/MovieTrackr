// #pragma warning disable SKEXP0001 , SKEXP0110
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MovieTrackR.Domain.Entities.AI;
using MovieTrackR.Domain.Enums.AI;
using MovieTrackR.AI.Builder;

namespace MovieTrackR.AI.Agents.IntentExtractorAgent;

public sealed class IntentExtractor(SemanticKernelBuilder builder)
{
    private readonly Kernel _kernel =
        builder.BuildKernel(IntentExtractorAgentProperties.Service);

    private ChatCompletionAgent BuildAgent()
    {
        return new ChatCompletionAgent
        {
            Name = IntentExtractorAgentProperties.Name,
            Description = IntentExtractorAgentProperties.Description,
            Instructions = IntentExtractorAgentProperties.Instructions,
            Kernel = _kernel,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings()
                {
                    ServiceId = IntentExtractorAgentProperties.Service,
                    MaxTokens = 100,
                    Temperature = 0.7
                }
            )
        };
    }

    /// <summary>
    /// Analyse la requête utilisateur et identifie l’intention principale.
    /// </summary>
    public async Task<IntentResponse> ExtractIntent(ChatHistory chatHistory)
    {
        ChatHistory agentChatHistory = FewShotExamples();
        agentChatHistory.Add(chatHistory.Last());
        ChatCompletionAgent intentExtractorAgent = BuildAgent();
        try
        {
            await foreach (ChatMessageContent response in intentExtractorAgent.InvokeAsync(agentChatHistory))
            {
                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    try
                    {
                        IntentResponseData? jsonData = JsonSerializer.Deserialize<IntentResponseData>(
                            response.Content,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (jsonData != null && jsonData.Intents.Any())
                        {
                            // IntentType intentType = Enum.TryParse(jsonData.Intent, true, out IntentType parsedIntent)
                            //     ? parsedIntent
                            //     : IntentType.None;
                            List<IntentProcessingStep> intentSteps = jsonData.Intents.Select(intent =>
                                new IntentProcessingStep(Enum.TryParse(intent.IntentType, true, out IntentType parsedIntent) ? parsedIntent : IntentType.None,
                                intent.AdditionalContext
                            )).ToList();

                            string clarifySentence = jsonData.ClarifySentence ?? "";
                            return new IntentResponse(intentSteps, clarifySentence);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"❌ Error parsing intent JSON response: {ex.Message}");
                        return new IntentResponse(new List<IntentProcessingStep>(), "❌ Unable to determine intent.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error while extracting intent: {ex.Message}");
            throw;
        }

        return new IntentResponse(new List<IntentProcessingStep>(), "❌ No response from intent agent.");
    }

    /// <summary>
    /// Ajoute un exemple de conversation pour l'entraînement du modèle et de l'agent.
    /// </summary>
    private ChatHistory FewShotExamples()
    {
        return new ChatHistory
        {
            // new ChatMessageContent(AuthorRole.User, "Can you delete a user?"),
            // new ChatMessageContent(AuthorRole.Assistant, "{\"intent\": \"None\", \"clarify_sentence\": \"I'm sorry can't help with that. I can review your account details, transactions and help you with your payments.\"}"),
            new ChatMessageContent(AuthorRole.User, "Can you show me the users?"),
            new ChatMessageContent(AuthorRole.Assistant,
                "{\"intents\": [{\"intent_type\": \"UserAgent\", \"additional_context\": null}], \"clarify_sentence\": \"User wants to get all the users as a list\"}"),

            new ChatMessageContent(AuthorRole.User, "Who's the last created person?"),
            new ChatMessageContent(AuthorRole.Assistant,
                "{\"intents\": [{\"intent_type\": \"UserAgent\", \"additional_context\": null}], \"clarify_sentence\": \"User wants to get the last user by created date\"}"),

            new ChatMessageContent(AuthorRole.User, "Delete this user please"),
            new ChatMessageContent(AuthorRole.Assistant,
                "{\"intents\": [{\"intent_type\": \"UserAgent\", \"additional_context\": \"last_created_user_id\"}], \"clarify_sentence\": \"User wants to delete the last created user\"}"),

            new ChatMessageContent(AuthorRole.User, "Where's the station currently located?"),
            new ChatMessageContent(AuthorRole.Assistant,
            "{\"intents\": [{\"intent_type\": \"IssAgent\", \"additional_context\": null}], \"clarify_sentence\": \"Retrieving the ISS position\"}"),

            new ChatMessageContent(AuthorRole.User, "Do you know about the game The Last Of Us?"),
            new ChatMessageContent(AuthorRole.Assistant,
                "{\"intents\": [{\"intent_type\": \"BingAgent\", \"additional_context\": null}], \"clarify_sentence\": \"User is looking for information about The Last Of Us\"}"),

            new ChatMessageContent(AuthorRole.User, "Can you search for Semantic Kernel information?"),
            new ChatMessageContent(AuthorRole.Assistant,
                "{\"intents\": [{\"intent_type\": \"BingAgent\", \"additional_context\": null}], \"clarify_sentence\": \"User is looking for information about Semantic Kernel Technology\"}")
        };
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
// #pragma warning restore SKEXP0001, SKEXP0110