using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Tapio.Application.Common.Helpers;

public static partial class SlugHelper
{
    public static string Generate(string name)
    {
        var normalized = name.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC);
        slug = slug.Replace('ı', 'i').Replace('ğ', 'g').Replace('ü', 'u').Replace('ş', 's').Replace('ö', 'o').Replace('ç', 'c');
        slug = NonAlphaNumeric().Replace(slug, "-");
        slug = MultiDash().Replace(slug, "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "business" : slug;
    }

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex NonAlphaNumeric();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultiDash();
}
