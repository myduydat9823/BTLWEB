using BTLWEB.Services;

namespace BTLWEB.Tests.Services;

public class HtmlSanitizerServiceTests
{
    private readonly HtmlSanitizerService _htmlSanitizerService = new();

    [Fact]
    public void Sanitize_ShouldRemoveScriptTags()
    {
        var sanitized = _htmlSanitizerService.Sanitize("<p>Hello</p><script>alert('xss')</script>");

        Assert.Contains("<p>Hello</p>", sanitized);
        Assert.DoesNotContain("<script", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sanitize_ShouldRemoveJavaScriptEventAttributes()
    {
        var sanitized = _htmlSanitizerService.Sanitize("""<img src="x" onerror="alert('xss')">""");

        Assert.Contains("<img", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sanitize_ShouldRemoveJavaScriptUrls()
    {
        var sanitized = _htmlSanitizerService.Sanitize("""<a href="javascript:alert('xss')">Click</a>""");

        Assert.Contains("Click", sanitized);
        Assert.DoesNotContain("javascript:", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sanitize_ShouldKeepSafeFormattingTags()
    {
        var sanitized = _htmlSanitizerService.Sanitize("<h2>Tiêu đề</h2><p>Nội dung <strong>đậm</strong></p><ul><li>Mục</li></ul>");

        Assert.Contains("<h2>Tiêu đề</h2>", sanitized);
        Assert.Contains("<strong>đậm</strong>", sanitized);
        Assert.Contains("<li>Mục</li>", sanitized);
    }
}
