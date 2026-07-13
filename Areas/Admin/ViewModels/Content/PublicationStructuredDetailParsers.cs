using CAFRI.ViewModels.Publications;

namespace CAFRI.Areas.Admin.ViewModels.Content;

internal static class PublicationStructuredDetailParsers
{
    public static IReadOnlyList<string[]> ParsePipeRows(string? raw, int minSegments)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split('|', StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length >= minSegments)
            .ToList();
    }

    public static IReadOnlyList<string> ParseLineCollection(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    public static IReadOnlyList<string> ParseMultiValueSegment(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split("~~", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    public static string JoinActions(IEnumerable<PublicationActionItemViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => x.Label));

    public static string JoinParagraphs(IEnumerable<string> paragraphs) =>
        string.Join(Environment.NewLine, paragraphs);

    public static string JoinKeyFindings(IEnumerable<PublicationFindingCardViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Icon} | {x.Title} | {x.Description}"));

    public static string JoinSections(IEnumerable<PublicationArticleSectionViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x =>
        {
            var callout = x.Callout;
            return string.Join(" | ",
            [
                x.Id,
                x.Heading,
                string.Join(" ~~ ", x.Paragraphs),
                string.Join(" ~~ ", x.BulletPoints),
                callout?.Tone ?? string.Empty,
                callout?.Label ?? string.Empty,
                callout?.Title ?? string.Empty,
                callout?.Description ?? string.Empty
            ]);
        }));

    public static string JoinDocuments(IEnumerable<PublicationDocumentCardViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Title} | {x.Type} | {x.Meta} | {x.DownloadUrl}"));

    public static string JoinRelatedLinks(IEnumerable<PublicationSidebarLinkItemViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Title} | {x.Meta} | {x.Url}"));

    public static string JoinInfo(IEnumerable<PublicationInfoRowViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Label} | {x.Value}"));
}
