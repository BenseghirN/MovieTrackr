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
}
