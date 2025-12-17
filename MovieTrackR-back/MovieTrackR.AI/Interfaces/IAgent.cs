
using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Domain.Entities.AI;

namespace MovieTrackR.AI.Interfaces;

public interface IAgent
{
    Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, IntentProcessingStep? intentStep = null, CancellationToken cancellationToken = default);
}

