using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Application.AI.Interfaces;
using MovieTrackR.Domain.Entities.AI;
using MovieTrackR.Domain.Enums.AI;
using MovieTrackR.AI.Interfaces;
using MovieTrackR.AI.Agents.IntentExtractorAgent;

namespace MovieTrackR.AI.Agents.RouteurAgent;

public sealed class Routeur(
    IntentExtractor intentExtractor,
    IPersonSeekerAgent personSeekerAgent,
    IRedactorAgent redactor) : IRouteurAgent
{
    /// <summary>
    /// Analyse Request from user and extrat the intent to dispatch it through right agent
    /// </summary>
    public async Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, CancellationToken cancellationToken = default)
    {
        if (agentContext == null) agentContext = new AgentContext();
        IntentResponse intentResponse;
        // In case of web search force
        // if (agentContext.WebSearch == true)
        // {

        //     await Task.FromResult(
        //     intentResponse = new IntentResponse(
        //         new List<IntentProcessingStep>
        //         {
        //             new IntentProcessingStep(IntentType.BingAgent, null)
        //         }, "User want to search on the web for some informations about his input"));
        // }
        // else
        // {
        intentResponse = await intentExtractor.ExtractIntent(chatHistory);
        // intentResponse = new IntentResponse(
        //     intents: new List<IntentProcessingStep>
        //     {
        //             new IntentProcessingStep(
        //                 IntentType.PersonSeekerAgent,
        //                 additionalContext: "name=Keanu Reeves"
        //             )
        //     },
        //     message: "Search for person Keanu Reeves"
        // );
        // }
        await RouteToAgent(intentResponse, chatHistory, agentContext, cancellationToken);
    }

    private async Task RouteToAgent(IntentResponse intentResponse, ChatHistory chatHistory, AgentContext agentContext, CancellationToken cancellationToken)
    {
        var responses = new List<string>();

        foreach (IntentProcessingStep step in intentResponse.Intents)
        {
            switch (step.IntentType)
            {
                // case IntentType.UserAgent:
                // await _userAgent.ProcessRequestAsync(chatHistory, agentContext, intentResponse);
                // chatHistory.AddAssistantMessage(agentContext.Result);
                // break;
                // case IntentType.IssAgent:
                // await _issAgent.ProcessRequestAsync(chatHistory, agentContext, intentResponse);
                // chatHistory.AddAssistantMessage(agentContext.Result);
                // break;
                // case IntentType.BingAgent:
                // await _bingAgent.ProcessRequestAsync(chatHistory, agentContext, intentResponse);
                // chatHistory.AddAssistantMessage(agentContext.Result);
                // break;
                // case IntentType.OcrAgent:
                // await _ocrAgent.ProcessRequestAsync(chatHistory, agentContext, intentResponse);
                // chatHistory.AddAssistantMessage(agentContext.Result);
                // break;
                case IntentType.PersonSeekerAgent:
                    await personSeekerAgent.ProcessRequestAsync(chatHistory, agentContext, intentResponse, cancellationToken);
                    chatHistory.AddAssistantMessage(agentContext.Result);
                    break;
                case IntentType.None:
                    chatHistory.AddAssistantMessage(intentResponse.Message ?? "Désolé je n'ai pas compris votre demande.");
                    return;
                default:
                    chatHistory.AddAssistantMessage("Impossible de traiter votre demande.");
                    return;
            }

            if (!string.IsNullOrEmpty(agentContext.Result))
            {
                chatHistory.AddAssistantMessage(agentContext.Result);
                responses.Add($"[{step.IntentType}] {agentContext.Result}");
            }
        }

        // if (responses.Count > 0)
        // {
        //     agentContext.Add("combinedResponses", string.Join("\n", responses));
        //     await redactor.ProcessRequestAsync(chatHistory, agentContext, cancellationToken: cancellationToken);
        //     chatHistory.AddAssistantMessage(agentContext.Result);
        // }
    }
}
