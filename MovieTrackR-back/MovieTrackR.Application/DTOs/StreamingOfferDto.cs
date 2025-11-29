namespace MovieTrackR.Application.DTOs;
/// <summary>
/// 
/// </summary>
public class StreamingOfferDto
{
    public string Country { get; set; } = string.Empty;
    public string? Link { get; set; }
    public IReadOnlyList<WatchProviderDto>? Flatrate { get; set; }
    public IReadOnlyList<WatchProviderDto>? Free { get; set; }
}

public class WatchProviderDto
{
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string LogoPath { get; set; } = string.Empty;
}