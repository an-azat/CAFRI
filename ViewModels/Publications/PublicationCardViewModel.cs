using System.Text.RegularExpressions;

namespace CAFRI.ViewModels.Publications;

public sealed partial class PublicationCardViewModel
{
    public required string Type { get; init; }

    public required string CountryCode { get; init; }

    public required string CountryLabel { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Meta { get; init; }

    public required string Slug { get; init; }

    public string Topic { get; init; } = string.Empty;

    public string? SourceName { get; init; }

    public string? AuthorLabel { get; init; }

    public string? SourceUrl { get; init; }

    public string? PdfDownloadUrl { get; init; }

    public string DocumentLabel { get; init; } = string.Empty;

    public string? HeroImageUrl { get; init; }

    // First downloadable file from the publication's documents (set by the listing query).
    public string? DownloadUrl { get; set; }

    public string DownloadType { get; set; } = "PDF";

    public string DownloadSize { get; set; } = string.Empty;

    public string Url => $"/publications/{Slug}";

    public string AccentClass => CountryCode switch
    {
        "KZ" => "country-badge--kz",
        "UZ" => "country-badge--uz",
        "KG" => "country-badge--kg",
        "TJ" => "country-badge--tj",
        "TM" => "country-badge--tm",
        _ => "country-badge--default"
    };

    // Meta is authored as "<date> | <reading time>", e.g. "May 2026 | 25 min read".
    public string DateLabel => MetaPart(0) is { Length: > 0 } date && !date.Equals("TBD", StringComparison.OrdinalIgnoreCase)
        ? date
        : string.Empty;

    public string ReadingTime => MetaPart(1);

    public string SourceLabel =>
        !string.IsNullOrWhiteSpace(SourceName) ? SourceName.Trim()
        : !string.IsNullOrWhiteSpace(AuthorLabel) ? AuthorLabel.Trim()
        : "CAFRI Research";

    public bool HasImage => !string.IsNullOrWhiteSpace(HeroImageUrl);

    // The legacy field holds "#" placeholders and, for imported items, the source article URL,
    // so only treat it as a file when it actually points at a PDF.
    private bool HasLegacyPdf => !string.IsNullOrWhiteSpace(PdfDownloadUrl)
        && PdfDownloadUrl.Trim().Split('?', '#')[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

    public string? FileUrl => !string.IsNullOrWhiteSpace(DownloadUrl) ? DownloadUrl : HasLegacyPdf ? PdfDownloadUrl!.Trim() : null;

    public bool HasFile => FileUrl is not null;

    public string FileTypeLabel => !string.IsNullOrWhiteSpace(DownloadUrl) ? DownloadType : "PDF";

    public bool HasOriginalSource => Uri.TryCreate(SourceUrl, UriKind.Absolute, out _);

    // Uploaded documents carry their own size; the legacy PDF field only has it inside
    // DocumentLabel, e.g. "PDF available | 1.2 MB".
    public string FileSizeLabel => !string.IsNullOrWhiteSpace(DownloadUrl)
        ? DownloadSize
        : FileSizePattern().Match(DocumentLabel) is { Success: true } match ? match.Value : string.Empty;

    public string CoverVariant => Type.ToUpperInvariant() switch
    {
        "REGULATORY NOTE" or "SANCTIONS ANALYSIS" => "pub-cover--reg",
        "COUNTRY PROFILE" or "BANKING REVIEW" => "pub-cover--report",
        "RESEARCH PAPER" or "ANALYTICAL BRIEF" => "pub-cover--data",
        _ => string.Empty
    };

    private string MetaPart(int index)
    {
        var parts = Meta.Split('|', StringSplitOptions.TrimEntries);
        return index < parts.Length ? parts[index] : string.Empty;
    }

    [GeneratedRegex(@"\d+(?:[.,]\d+)?\s?(?:KB|MB|GB)", RegexOptions.IgnoreCase)]
    private static partial Regex FileSizePattern();
}
