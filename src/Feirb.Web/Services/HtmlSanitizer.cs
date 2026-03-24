using Ganss.Xss;

namespace Feirb.Web.Services;

public static class HtmlSanitizer
{
    private static readonly Ganss.Xss.HtmlSanitizer _sanitizer = CreateSanitizer();

    public static string Sanitize(string? html) =>
        string.IsNullOrWhiteSpace(html) ? string.Empty : _sanitizer.Sanitize(html);

    private static Ganss.Xss.HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new Ganss.Xss.HtmlSanitizer();

        // Only allow safe URI schemes
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("mailto");
        sanitizer.AllowedSchemes.Add("data");

        // Remove tags that load external content or allow interaction
        sanitizer.AllowedTags.Remove("video");
        sanitizer.AllowedTags.Remove("audio");
        sanitizer.AllowedTags.Remove("source");
        sanitizer.AllowedTags.Remove("picture");
        sanitizer.AllowedTags.Remove("link");
        sanitizer.AllowedTags.Remove("form");
        sanitizer.AllowedTags.Remove("input");
        sanitizer.AllowedTags.Remove("button");
        sanitizer.AllowedTags.Remove("select");
        sanitizer.AllowedTags.Remove("textarea");

        // Block external image sources — only allow data: URIs
        sanitizer.FilterUrl += (_, e) =>
        {
            if (e.OriginalUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return;

            // Block external URLs on img tags
            if (string.Equals(e.Tag.LocalName, "img", StringComparison.OrdinalIgnoreCase))
                e.SanitizedUrl = null;
        };

        return sanitizer;
    }
}
