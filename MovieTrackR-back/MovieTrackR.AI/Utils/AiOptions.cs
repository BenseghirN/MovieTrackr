namespace MovieTrackR.AI.Utils;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public string EndpointUrl { get; init; } = default!;
    public string ApiKey { get; init; } = default!;
    public string ModelName { get; init; } = default!;
    public static string KernelService = "MovieTrackR";
}
