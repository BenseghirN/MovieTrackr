using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Domain.Entities.IA;

namespace MovieTrackR.Application.IA.Interfaces;

public interface IRouteurAgent
{
    Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext);
}