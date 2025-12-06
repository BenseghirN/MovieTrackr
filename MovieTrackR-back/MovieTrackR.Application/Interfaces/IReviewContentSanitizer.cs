using Ganss.Xss;

namespace MovieTrackR.Application.Interfaces;

public interface IReviewContentSanitizer
{
    string Sanitize(string html);
}

public class ReviewContentSanitizer : IReviewContentSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public ReviewContentSanitizer()
    {
        _sanitizer = new HtmlSanitizer();

        _sanitizer.AllowedTags.Add("p");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("ul");
        _sanitizer.AllowedTags.Add("ol");
        _sanitizer.AllowedTags.Add("li");
        _sanitizer.AllowedTags.Add("blockquote");
        _sanitizer.AllowedTags.Add("img");
        _sanitizer.AllowedTags.Add("a");
        _sanitizer.AllowedTags.Add("br");

        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedAttributes.Add("src");
        _sanitizer.AllowedAttributes.Add("alt");
        _sanitizer.AllowedAttributes.Add("title");

        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("data");
    }

    public string Sanitize(string html) => _sanitizer.Sanitize(html ?? string.Empty);
}
