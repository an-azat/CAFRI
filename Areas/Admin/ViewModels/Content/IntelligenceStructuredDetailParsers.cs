using CAFRI.ViewModels.Intelligence;

namespace CAFRI.Areas.Admin.ViewModels.Content;

internal static class IntelligenceStructuredDetailParsers
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

    public static string JoinKeyChanges(IEnumerable<IntelligenceKeyChangeViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Number} | {x.Title} | {x.Description}"));

    public static string JoinImpactAnalysis(IEnumerable<IntelligenceImpactCardViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Title} | {x.Description} | {x.ImpactLabel} | {x.ImpactTone}"));

    public static string JoinTimeline(IEnumerable<IntelligenceTimelineItemViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.DateLabel} | {x.Description} | {x.Stage}"));

    public static string JoinDocumentInfo(IEnumerable<IntelligenceDocumentInfoRowViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Label} | {x.Value}"));

    public static string JoinLinks(IEnumerable<IntelligenceSidebarLinkViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Title} | {x.Subtitle} | {x.Url}"));

    public static string JoinStatus(IEnumerable<IntelligenceStatusRowViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Label} | {x.Value} | {x.IsStatus}"));
}
