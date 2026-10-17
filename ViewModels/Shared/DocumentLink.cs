using System.Text.RegularExpressions;

namespace CAFRI.ViewModels.Shared;

// Documents saved by the admin before the row-format fix stored
// "<size> | <url>" in DownloadUrl (e.g. "431,3 KB | /uploads/.../file.pdf").
// The URL is always the last " | " segment, so it can be recovered reliably.
public static partial class DocumentLink
{
    public static string Url(string? rawDownloadUrl)
    {
        if (string.IsNullOrWhiteSpace(rawDownloadUrl))
        {
            return "#";
        }

        var url = rawDownloadUrl.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        return string.IsNullOrWhiteSpace(url) ? "#" : url;
    }

    public static bool IsDownloadable(string? rawDownloadUrl) => Url(rawDownloadUrl) != "#";

    // Size label such as "431,3 KB", taken from the meta or the legacy download value.
    public static string SizeLabel(params string?[] sources) =>
        sources
            .Select(source => source is null ? null : FileSizePattern().Match(source))
            .FirstOrDefault(match => match is { Success: true })?.Value ?? string.Empty;

    [GeneratedRegex(@"\d+(?:[.,]\d+)?\s?(?:B|KB|MB|GB)\b", RegexOptions.IgnoreCase)]
    private static partial Regex FileSizePattern();
}
