namespace MovieTrackR.Domain.Entities.AI;

public class AgentContext : Dictionary<string, object>
{
    public AgentContext() : base() { }

    public AgentContext(string result) : base()
    {
        this["result"] = result;
    }

    public string Result
    {
        get => ContainsKey("result") ? this["result"] as string ?? string.Empty : string.Empty;
        set => this["result"] = value;
    }

    public string? AdditionalContext
    {
        get => ContainsKey("additionalContext") ? this["additionalContext"] as string ?? string.Empty : string.Empty;
        set
        {
            if (value == null)
                Remove("additionalContext");
            else
                this["additionalContext"] = value;
        }
    }

    public object? Attachments
    {
        get => TryGetValue("attachments", out var v) ? v : null;
        set
        {
            if (value == null) Remove("attachments");
            else this["attachments"] = value;
        }
    }
}
