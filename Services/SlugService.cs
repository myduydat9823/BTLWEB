using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BTLWEB.Services.Interfaces;

namespace BTLWEB.Services;

public sealed partial class SlugService : ISlugService
{
    private const string FallbackSlug = "bai-viet";

    public string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return FallbackSlug;
        }

        var normalized = title.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
            if (unicodeCategory == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(character == 'đ' ? 'd' : character);
        }

        var withoutDiacritics = builder.ToString().Normalize(NormalizationForm.FormC);
        var asciiOnly = InvalidSlugCharacterRegex().Replace(withoutDiacritics, "-");
        var collapsed = MultipleHyphenRegex().Replace(asciiOnly, "-").Trim('-');

        return string.IsNullOrWhiteSpace(collapsed) ? FallbackSlug : collapsed;
    }

    public async Task<string> GenerateUniqueSlugAsync(string title, Func<string, Task<bool>> slugExistsAsync)
    {
        ArgumentNullException.ThrowIfNull(slugExistsAsync);

        var baseSlug = GenerateSlug(title);
        var candidate = baseSlug;
        var suffix = 2;

        while (await slugExistsAsync(candidate))
        {
            candidate = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return candidate;
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex InvalidSlugCharacterRegex();

    [GeneratedRegex("-{2,}", RegexOptions.Compiled)]
    private static partial Regex MultipleHyphenRegex();
}
