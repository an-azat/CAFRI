using System.Net;
using System.Text.RegularExpressions;

namespace CAFRI.Infrastructure.Content;

/// <summary>
/// Builds the executive summary for publications that have no authored summary,
/// using the publication's own description from the database.
/// </summary>
public static class PublicationSummaryFallback
{
    // Fragments of the generic placeholder that older builds generated and that the
    // admin editor could persist back into DetailsJson or the executive-summary section.
    private static readonly string[] LegacyTemplateMarkers =
    [
        "provides a structured analytical overview of current developments relevant to",
        "This fallback detail layout is now generated from database content"
    ];

    public static List<string> BuildParagraphs(string? description) =>
        string.IsNullOrWhiteSpace(description) ? [] : [description.Trim()];

    public static bool IsLegacyTemplate(string? text) =>
        !string.IsNullOrWhiteSpace(text) &&
        LegacyTemplateMarkers.Any(marker => text.Contains(marker, StringComparison.OrdinalIgnoreCase));

    public static List<string> RemoveLegacyTemplate(IEnumerable<string> paragraphs) =>
        paragraphs.Where(paragraph => !IsLegacyTemplate(paragraph)).ToList();

    // Converts rich-text summary HTML (from the admin editor) into plain-text paragraphs,
    // so guest previews show the authored summary instead of the generic fallback.
    public static List<string> ExtractParagraphs(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return [];
        }

        var withBreaks = BlockBoundaryRegex.Replace(html, "\n");
        var text = WebUtility.HtmlDecode(TagRegex.Replace(withBreaks, " "));

        return text
            .Split('\n')
            .Select(line => WhitespaceRegex.Replace(line, " ").Trim())
            .Where(line => line.Length > 0)
            .ToList();
    }

    // Short teaser for the sign-in gate: the first three sentences of the summary
    // (two if three run too long), always ending with an ellipsis.
    public static string BuildPreview(IEnumerable<string> paragraphs, string? fallbackText)
    {
        var text = string.Join(" ", paragraphs.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()));
        if (string.IsNullOrWhiteSpace(text))
        {
            text = fallbackText?.Trim() ?? string.Empty;
        }

        if (text.Length == 0)
        {
            return string.Empty;
        }

        var sentences = SentenceRegex.Matches(text).Select(m => m.Value.Trim()).Where(s => s.Length > 0).ToList();
        var count = Math.Min(3, sentences.Count);
        if (count == 3 && string.Join(" ", sentences.Take(3)).Length > PreviewMaxLength)
        {
            count = 2;
        }

        var preview = count == 0 ? text : string.Join(" ", sentences.Take(count));
        return preview.TrimEnd('.', '!', '?', '…', ' ') + "...";
    }

    private const int PreviewMaxLength = 450;

    private static readonly Regex SentenceRegex = new(@"[^.!?…]+(?:[.!?…]+|$)", RegexOptions.Compiled);

    private static readonly Regex BlockBoundaryRegex =
        new(@"<\s*br\s*/?\s*>|<\s*/\s*(p|div|li|h[1-6]|blockquote)\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TagRegex = new(@"<[^>]+>", RegexOptions.Compiled);

    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
}
