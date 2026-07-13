using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BTLWEB.ViewModels.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed partial class RequiredPlainTextAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string text)
        {
            return false;
        }

        var withoutTags = HtmlTagRegex().Replace(text, " ");
        var withoutEntities = HtmlEntityRegex().Replace(withoutTags, " ");

        return !string.IsNullOrWhiteSpace(withoutEntities);
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("&[a-zA-Z0-9#]+;", RegexOptions.Compiled)]
    private static partial Regex HtmlEntityRegex();
}
