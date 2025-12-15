using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Domain.Entities.AI;

namespace MovieTrackR.Application.AI.Interfaces;

public interface IRouteurAgent
{
    Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, CancellationToken cancellationToken);
}