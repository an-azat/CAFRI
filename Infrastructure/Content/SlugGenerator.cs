using System.Globalization;
using System.Text;

namespace CAFRI.Infrastructure.Content;

/// <summary>
/// Builds URL slugs: lowercase ASCII letters, digits and single hyphens. Cyrillic (Russian and
/// Kazakh) is transliterated so the public URL stays readable. Keep in sync with the
/// slugify() function in Areas/Admin/Views/Publications/Edit.cshtml.
/// </summary>
public static class SlugGenerator
{
    public const int GeneratedMaxLength = 25;
    public const int ManualMaxLength = 180;

    private static readonly Dictionary<char, string> Transliteration = new()
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d", ['е'] = "e", ['ё'] = "e",
        ['ж'] = "zh", ['з'] = "z", ['и'] = "i", ['й'] = "y", ['к'] = "k", ['л'] = "l", ['м'] = "m",
        ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t", ['у'] = "u",
        ['ф'] = "f", ['х'] = "h", ['ц'] = "ts", ['ч'] = "ch", ['ш'] = "sh", ['щ'] = "sch", ['ъ'] = "",
        ['ы'] = "y", ['ь'] = "", ['э'] = "e", ['ю'] = "yu", ['я'] = "ya",
        ['ә'] = "a", ['ғ'] = "g", ['қ'] = "q", ['ң'] = "n", ['ө'] = "o", ['ұ'] = "u", ['ү'] = "u",
        ['һ'] = "h", ['і'] = "i"
    };

    // Turns any text into a slug of at most maxLength characters, cutting at a word boundary
    // when that keeps a reasonable length.
    public static string Slugify(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var character in text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (Transliteration.TryGetValue(character, out var latin))
            {
                builder.Append(latin);
            }
            else if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(character);
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var slug = builder.ToString().Trim('-');
        if (slug.Length <= maxLength)
        {
            return slug;
        }

        var cut = slug[..maxLength];
        var lastHyphen = cut.LastIndexOf('-');
        if (slug[maxLength] != '-' && lastHyphen >= maxLength / 2)
        {
            cut = cut[..lastHyphen];
        }

        return cut.Trim('-');
    }

    // Appends "-2", "-3", ... until isTaken says the slug is free, staying within maxLength.
    public static string MakeUnique(string slug, int maxLength, Func<string, bool> isTaken)
    {
        if (!isTaken(slug))
        {
            return slug;
        }

        for (var index = 2; ; index++)
        {
            var suffix = $"-{index}";
            var stem = slug.Length + suffix.Length > maxLength
                ? slug[..(maxLength - suffix.Length)].TrimEnd('-')
                : slug;
            var candidate = stem + suffix;
            if (!isTaken(candidate))
            {
                return candidate;
            }
        }
    }
}
