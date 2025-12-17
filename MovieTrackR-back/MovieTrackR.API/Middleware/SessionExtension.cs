using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MovieTrackR.API.Middleware;

public static class SessionHelper
{

    /// <summary>
    /// Extension method to save an object in the session as a JSON string.
    /// </summary>
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    /// <summary>
    /// Extension method to retrieve an object from the session as a JSON string.
    /// </summary>
    public static T? GetObjectFromJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }

    /// <summary>
    /// ChatHistory extension method to save the chat history in the session, because the session can only store strings.
    /// </summary>
    public static void SetChatHistory(this ISession session, string key, ChatHistory chatHistory)
    {
        List<KeyValuePair<string, string?>> messagesList = chatHistory.Select(msg => new KeyValuePair<string, string?>(msg.Role.Label, msg.Content)).ToList();
        session.SetObjectAsJson(key, messagesList);
    }

    /// <summary>
    /// ChatHistory extension method to retrieve the chat history from the session.
    /// </summary>
    public static ChatHistory GetChatHistory(this ISession session, string key)
    {
        try
        {
            var messagesList = session.GetObjectFromJson<List<KeyValuePair<string, string>>>(key);
            if (messagesList == null) return new ChatHistory();

            ChatHistory chatHistory = new ChatHistory();
            foreach (var entry in messagesList)
            {
                switch (entry.Key?.ToLower())
                {
                    case "user":
                        chatHistory.AddUserMessage(entry.Value);
                        break;
                    case "assistant":
                        chatHistory.AddAssistantMessage(entry.Value);
                        break;
                    case "tool":
                        chatHistory.AddMessage(AuthorRole.Tool, entry.Value);
                        break;
                    default:
                        chatHistory.AddSystemMessage(entry.Value);
                        break;
                }
            }
            return chatHistory;
        }
        catch (Exception)
        {
            return new ChatHistory();
        }
    }
    /// <summary>
    /// Extension method to reset the chat history in the session.
    /// </summary>
    public static void ResetChatHistory(this ISession session, string key)
    {
        session.Remove(key);
    }
}