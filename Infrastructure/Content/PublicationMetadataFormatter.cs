using System.Globalization;

namespace CAFRI.Infrastructure.Content;

public static class PublicationMetadataFormatter
{
    public static readonly string[] CanonicalReadingTimes =
    [
        "8 min read",
        "10 min read",
        "11 min read",
        "12 min read",
        "14 min read",
        "16 min read",
        "18 min read",
        "20 min read",
        "25 min read"
    ];

    public static string NormalizePublishedDate(string? publishedMonth, string? publishedDate, string? meta)
    {
        if (!string.IsNullOrWhiteSpace(publishedMonth) &&
            DateTime.TryParseExact(publishedMonth.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthValue))
        {
            return monthValue.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrWhiteSpace(publishedDate))
        {
            var trimmed = publishedDate.Trim();
            if (DateTime.TryParseExact(trimmed, ["MMMM yyyy", "MMM yyyy", "yyyy-MM", "yyyy-MM-dd"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return parsed.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            }

            return trimmed;
        }

        if (!string.IsNullOrWhiteSpace(meta))
        {
            return meta.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        }

        return string.Empty;
    }

    public static string NormalizeReadingTime(string? readingTime, string? meta)
    {
        if (!string.IsNullOrWhiteSpace(readingTime))
        {
            return readingTime.Trim();
        }

        if (!string.IsNullOrWhiteSpace(meta) && meta.Contains('|', StringComparison.Ordinal))
        {
            return meta.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault() ?? "12 min read";
        }

        return "12 min read";
    }

    public static string BuildMeta(string publishedDate, string readingTime)
    {
        var left = string.IsNullOrWhiteSpace(publishedDate) ? "TBD" : publishedDate.Trim();
        var right = string.IsNullOrWhiteSpace(readingTime) ? "12 min read" : readingTime.Trim();
        return $"{left} | {right}";
    }

    public static string ToMonthInputValue(string? publishedDate)
    {
        if (string.IsNullOrWhiteSpace(publishedDate))
        {
            return string.Empty;
        }

        if (DateTime.TryParseExact(publishedDate.Trim(), ["MMMM yyyy", "MMM yyyy", "yyyy-MM", "yyyy-MM-dd"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }

        return string.Empty;
    }
}
