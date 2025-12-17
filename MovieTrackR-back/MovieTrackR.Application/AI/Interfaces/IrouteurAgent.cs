using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Domain.Entities.AI;

namespace MovieTrackR.Application.AI.Interfaces;

public interface IRouteur
{
    Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, CancellationToken cancellationToken);
}