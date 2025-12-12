using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.Domain.Entities.IA;

namespace MovieTrackR.IA.Interfaces;

public interface IAgent
{
    Task ProcessRequestAsync(ChatHistory chatHistory, AgentContext agentContext, IntentResponse? intentResponse = null);
}

