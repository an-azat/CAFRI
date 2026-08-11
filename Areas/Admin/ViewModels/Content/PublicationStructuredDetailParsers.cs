using CAFRI.ViewModels.Publications;

namespace CAFRI.Areas.Admin.ViewModels.Content;

using System.Globalization;

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

    public static IReadOnlyList<PublicationChartViewModel> ParseCharts(string? raw)
    {
        var rows = ParsePipeRows(raw, 5);
        var charts = new List<PublicationChartViewModel>();

        foreach (var parts in rows)
        {
            var seriesRaw = parts.Length > 5 ? string.Join(" | ", parts.Skip(5)) : string.Empty;
            var seriesItems = seriesRaw
                .Split("||", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(item => item.Split("@@", StringSplitOptions.TrimEntries))
                .Where(itemParts => itemParts.Length >= 3)
                .Select(itemParts => new PublicationChartSeriesViewModel
                {
                    Label = itemParts[0],
                    Color = itemParts[1],
                    Values = itemParts[2]
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(value => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : (decimal?)null)
                        .Where(value => value.HasValue)
                        .Select(value => value!.Value)
                        .ToList()
                })
                .Where(item => item.Values.Count != 0)
                .ToList();

            charts.Add(new PublicationChartViewModel
            {
                Title = parts[0],
                Subtitle = parts[1],
                ChartType = parts[2],
                ValueSuffix = parts[3],
                Labels = ParseMultiValueSegment(parts[4]),
                Series = seriesItems
            });
        }

        return charts;
    }

    public static string JoinCharts(IEnumerable<PublicationChartViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(item =>
            string.Join(" | ",
            [
                item.Title,
                item.Subtitle,
                item.ChartType,
                item.ValueSuffix,
                string.Join(" ~~ ", item.Labels),
                string.Join(" || ", item.Series.Select(series =>
                    $"{series.Label} @@ {series.Color} @@ {string.Join(", ", series.Values.Select(value => value.ToString(CultureInfo.InvariantCulture)))}"))
            ])));

    public static IReadOnlyList<PublicationTableRowViewModel> ParseTableRows(string? raw) =>
        ParsePipeRows(raw, 6)
            .Select(parts => new PublicationTableRowViewModel
            {
                Corridor = parts[0],
                CountriesCovered = parts[1],
                MainRoute = parts[2],
                Volume = parts[3],
                Growth = parts[4],
                Status = string.Join(" | ", parts.Skip(5))
            })
            .ToList();

    public static string JoinTableRows(IEnumerable<PublicationTableRowViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(item =>
            $"{item.Corridor} | {item.CountriesCovered} | {item.MainRoute} | {item.Volume} | {item.Growth} | {item.Status}"));

    public static string JoinDocuments(IEnumerable<PublicationDocumentCardViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Title} | {x.Type} | {x.Meta} | {x.DownloadUrl}"));

    public static string JoinRelatedLinks(IEnumerable<PublicationSidebarLinkItemViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Title} | {x.Meta} | {x.Url}"));

    public static string JoinInfo(IEnumerable<PublicationInfoRowViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Label} | {x.Value}"));
}
