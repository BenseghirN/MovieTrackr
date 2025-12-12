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
        get => this.ContainsKey("result") ? this["result"] as string ?? string.Empty : string.Empty;
        set => this["result"] = value;
    }

    public string Format
    {
        get => this.ContainsKey("format") ? this["format"] as string ?? string.Empty : string.Empty;
        set => this["format"] = value;
    }

    public bool WebSearch
    {
        get => this.ContainsKey("WebSearch") ? (bool)this["WebSearch"] : false;
        set => this["WebSearch"] = value;
    }

    public List<string> WebSources
    {
        get => this.ContainsKey("WebSources") ? this["WebSources"] as List<string> ?? new List<string>() : new List<string>();
        set => this["WebSources"] = value;
    }

    public object? AdditionalContext
    {
        get => this.ContainsKey("additionalContext") ? this["additionalContext"] : null;
        set => this["additionalContext"] = value!;
    }
}
