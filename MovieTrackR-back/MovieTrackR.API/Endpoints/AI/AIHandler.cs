using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MovieTrackR.API.Middleware;
using MovieTrackR.Application.AI.Contracts;
using MovieTrackR.Application.AI.Queries;

namespace MovieTrackR.API.Endpoints.AI;

public static class AIHandlers
{
    public static async Task<IResult> Chat(HttpContext httpCtx, [FromBody] ChatAppRequest chatRequest, IMediator mediator, CancellationToken cancellationToken)
    {
        if (chatRequest.Messages.Count == 0)
            return TypedResults.BadRequest("L'historique du chat ne peut pas être vide.");

        try
        {
            Console.WriteLine($"📌 Session ID: {httpCtx.Session.Id}");

            ChatHistory chatHistory = httpCtx.Session.GetChatHistory("ChatHistory") ?? new ChatHistory();
            chatHistory.AddRange(ConvertChatHistory(chatRequest));

            if (chatHistory.Count > 100)
            {
                List<ChatMessageContent> keep = chatHistory.Skip(chatHistory.Count - 100).ToList();
                chatHistory.Clear();
                foreach (ChatMessageContent m in keep) chatHistory.Add(m);
            }

            ChatResponse response = await mediator.Send(new ChatAppQuery(chatRequest, chatHistory), cancellationToken);

            httpCtx.Session.SetChatHistory("ChatHistory", chatHistory);
            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Une erreur inattendue est survenue: {ex.Message}");
        }
    }

    public static IResult ResetChatHistory(HttpContext httpCtx)
    {
        try
        {
            httpCtx.Session.ResetChatHistory("ChatHistory");
            DeleteSessionDirectory(httpCtx);
            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Une erreur inattendue est survenue: {ex.Message}");
        }
    }

    private static ChatHistory ConvertChatHistory(ChatAppRequest chatRequest)
    {
        ChatHistory chatHistory = new();

        foreach (ChatMessage message in chatRequest.Messages)
        {
            if (string.IsNullOrWhiteSpace(message.Content)) continue;

            if (message.Role?.ToLower() == "user")
                chatHistory.AddUserMessage(message.Content);
            else if (message.Role?.ToLower() == "assistant")
                chatHistory.AddAssistantMessage(message.Content);
        }

        return chatHistory;
    }

    private static void DeleteSessionDirectory(HttpContext httpContext)
    {
        string sessionDirectory = Path.Combine(Directory.GetCurrentDirectory(), "../files", httpContext.Session.Id);
        if (Directory.Exists(sessionDirectory))
        {
            Directory.Delete(sessionDirectory, true);
        }
    }
}
