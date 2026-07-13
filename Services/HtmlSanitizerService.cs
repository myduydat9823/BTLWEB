using BTLWEB.Services.Interfaces;
using Ganss.Xss;

namespace BTLWEB.Services;

public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlSanitizerService()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedSchemes.Add("data");
        _sanitizer.AllowedTags.Remove("iframe");
    }

    public string Sanitize(string? html)
    {
        return string.IsNullOrWhiteSpace(html) ? string.Empty : _sanitizer.Sanitize(html);
    }
}
