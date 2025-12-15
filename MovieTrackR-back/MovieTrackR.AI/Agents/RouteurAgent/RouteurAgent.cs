using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Application.AI.Interfaces;
using MovieTrackR.Domain.Entities.AI;
using MovieTrackR.Domain.Enums.AI;
using MovieTrackR.AI.Interfaces;
using MovieTrackR.AI.Agents.IntentExtractorAgent;

namespace MovieTrackR.AI.Agents.RouteurAgent;

public sealed class Routeur(
    IntentExtractor intentExtractor,
    IPersonSeekerAgent personSeekerAgent
    // IRedactorAgent redactor
    ) : IRouteurAgent
{
    public async Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, CancellationToken cancellationToken)
    {
        if (agentContext == null) agentContext = new AgentContext();
        IntentResponse intentResponse = await intentExtractor.ExtractIntent(chatHistory, agentContext, cancellationToken);
        await RouteToAgent(intentResponse, chatHistory, agentContext, cancellationToken);
    }

    private async Task RouteToAgent(IntentResponse intentResponse, ChatHistory chatHistory, AgentContext agentContext, CancellationToken cancellationToken)
    {
        List<string> responses = new List<string>();

        foreach (IntentProcessingStep step in intentResponse.Intents)
        {
            agentContext.Result = string.Empty;
            switch (step.IntentType)
            {
                case IntentType.PersonSeekerAgent:
                    await personSeekerAgent.ProcessRequestAsync(chatHistory, agentContext, step, cancellationToken);
                    break;
                case IntentType.None:
                    chatHistory.AddAssistantMessage(intentResponse.Message ?? "Désolé je n'ai pas compris votre demande.");
                    agentContext.AdditionalContext = null;
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
