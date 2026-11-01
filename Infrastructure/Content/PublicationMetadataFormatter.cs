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

    // Value format of <input type="datetime-local">; the admin enters UTC.
    public const string DateTimeInputFormat = "yyyy-MM-ddTHH:mm";

    private static readonly string[] LegacyDateFormats =
    [
        "MMMM yyyy", "MMM yyyy", "yyyy-MM", "yyyy-MM-dd", "d MMMM yyyy", "MMMM d, yyyy", "MMM d, yyyy", DateTimeInputFormat
    ];

    public static string FormatPublishedDate(DateTimeOffset? publishedAtUtc) =>
        publishedAtUtc?.UtcDateTime.ToString("d MMMM yyyy", CultureInfo.InvariantCulture) ?? string.Empty;

    // Compact form used by Intelligence cards, e.g. "Jun 15, 2026".
    public static string FormatShortDate(DateTimeOffset? publishedAtUtc) =>
        publishedAtUtc?.UtcDateTime.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? string.Empty;

    public static string ToDateTimeInputValue(DateTimeOffset? publishedAtUtc) =>
        publishedAtUtc?.UtcDateTime.ToString(DateTimeInputFormat, CultureInfo.InvariantCulture) ?? string.Empty;

    // Parses admin input ("yyyy-MM-ddTHH:mm") and the free-text dates older data used
    // ("June 2026", "2026-06-15", ...), always as UTC.
    public static DateTimeOffset? ParsePublishedDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (DateTime.TryParseExact(trimmed, LegacyDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var exact))
        {
            return new DateTimeOffset(DateTime.SpecifyKind(exact, DateTimeKind.Utc));
        }

        return DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed.ToUniversalTime()
            : null;
    }

    // Older rows keep the date only as the first "|" segment of Meta ("June 2026 | 12 min read").
    public static DateTimeOffset? ParsePublishedDateFromMeta(string? meta) =>
        string.IsNullOrWhiteSpace(meta)
            ? null
            : ParsePublishedDate(meta.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());

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

    public static string BuildMeta(DateTimeOffset? publishedAtUtc, string readingTime)
    {
        var left = publishedAtUtc is null ? "TBD" : FormatPublishedDate(publishedAtUtc);
        var right = string.IsNullOrWhiteSpace(readingTime) ? "12 min read" : readingTime.Trim();
        return $"{left} | {right}";
    }
}
