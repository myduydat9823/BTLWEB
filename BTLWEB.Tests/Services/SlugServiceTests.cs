using BTLWEB.Services;

namespace BTLWEB.Tests.Services;

public class SlugServiceTests
{
    private readonly SlugService _slugService = new();

    [Theory]
    [InlineData("Triển lãm ảnh nghệ thuật Việt Nam năm 2026", "trien-lam-anh-nghe-thuat-viet-nam-nam-2026")]
    [InlineData("Đời sống nhiếp ảnh", "doi-song-nhiep-anh")]
    public void GenerateSlug_ShouldRemoveVietnameseDiacritics(string title, string expected)
    {
        var slug = _slugService.GenerateSlug(title);

        Assert.Equal(expected, slug);
    }

    [Fact]
    public void GenerateSlug_ShouldRemoveSpecialCharacters()
    {
        var slug = _slugService.GenerateSlug("Ảnh đẹp!!! @ Việt Nam #2026");

        Assert.Equal("anh-dep-viet-nam-2026", slug);
    }

    [Fact]
    public void GenerateSlug_ShouldNotContainConsecutiveHyphens()
    {
        var slug = _slugService.GenerateSlug("  Ảnh --- đẹp    Việt,,, Nam  ");

        Assert.Equal("anh-dep-viet-nam", slug);
        Assert.DoesNotContain("--", slug);
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_ShouldAppendSuffixWhenSlugExists()
    {
        var existingSlugs = new HashSet<string>
        {
            "trien-lam-anh",
            "trien-lam-anh-2"
        };

        var slug = await _slugService.GenerateUniqueSlugAsync(
            "Triển lãm ảnh",
            candidate => Task.FromResult(existingSlugs.Contains(candidate)));

        Assert.Equal("trien-lam-anh-3", slug);
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_ShouldKeepSlugWhenCurrentArticleIsExcludedByCaller()
    {
        var slug = await _slugService.GenerateUniqueSlugAsync(
            "Triển lãm ảnh",
            _ => Task.FromResult(false));

        Assert.Equal("trien-lam-anh", slug);
    }
}
