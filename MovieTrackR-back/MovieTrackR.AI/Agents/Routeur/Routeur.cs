using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Application.AI.Interfaces;
using MovieTrackR.Domain.Entities.AI;
using MovieTrackR.Domain.Enums.AI;
using MovieTrackR.AI.Interfaces;
using MovieTrackR.AI.Agents.IntentExtractorAgent;

namespace MovieTrackR.AI.Agents.Routeur;

public sealed class Routeur(
    IntentExtractor intentExtractorAgent
    ,
    IPersonSeekerAgent personSeekerAgent,
    ISimilarMovieSeekerAgent similarMovieSeekerAgent,
    IDiscoverMoviesAgent discoverMoviesAgent,
    IRedactorAgent redactorAgent
    ) : IRouteur
{
    public async Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, CancellationToken cancellationToken)
    {
        if (agentContext == null) agentContext = new AgentContext();
        IntentResponse intentResponse = await intentExtractorAgent.ExtractIntent(chatHistory, agentContext, cancellationToken);
        await RouteToAgent(intentResponse, chatHistory, agentContext, cancellationToken);
    }

    private async Task RouteToAgent(IntentResponse intentResponse, ChatHistory chatHistory, AgentContext agentContext, CancellationToken cancellationToken)
    {
        List<string> responses = new List<string>();

        foreach (IntentProcessingStep step in intentResponse.Intents)
        {
            switch (step.IntentType)
            {
                case IntentType.DiscoverMovieAgent:
                    agentContext.Result = string.Empty;
                    await discoverMoviesAgent.ProcessRequestAsync(chatHistory, agentContext, step, cancellationToken);
                    break;
                case IntentType.PersonSeekerAgent:
                    agentContext.Result = string.Empty;
                    await personSeekerAgent.ProcessRequestAsync(chatHistory, agentContext, step, cancellationToken);
                    break;
                case IntentType.SimilarMovieSeekerAgent:
                    agentContext.Result = string.Empty;
                    await similarMovieSeekerAgent.ProcessRequestAsync(chatHistory, agentContext, step, cancellationToken);
                    break;
                case IntentType.ReviewRedactorAgent:
                    agentContext.Result = string.Empty;
                    await redactorAgent.ProcessRequestAsync(chatHistory, agentContext, step, cancellationToken);
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
    }
}
