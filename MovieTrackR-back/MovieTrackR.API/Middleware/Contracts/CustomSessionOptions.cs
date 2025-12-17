namespace MovieTrackR.API.Middleware.Contracts;

public class CustomSessionOptions
{
    public string IdleTimeout { get; set; } = string.Empty;
    public bool CookieHttpOnly { get; set; } = true;
    public bool CookieIsEssential { get; set; } = true;
}