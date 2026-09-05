using CAFRI.ViewModels.Home;

namespace CAFRI.Infrastructure.Content;

public static class HomePageContentTextSerializer
{
    public static IReadOnlyList<HomeCoverageAreaItemViewModel> ParseCoverageAreas(string? raw) =>
        ParsePipeRows(raw, 3)
            .Select(parts => new HomeCoverageAreaItemViewModel
            {
                Icon = parts[0],
                Title = parts[1],
                Description = string.Join(" | ", parts.Skip(2))
            })
            .ToList();

    // Number(0) | Title(1) | Description(2..) | Url | LinkLabel | [wide]
    // Description is treated as the free-text field: if an editor accidentally types
    // "|" inside the description, the extra segments are folded back into Description
    // instead of shifting Url/LinkLabel/IsWide out of position (see bug fix notes).
    public static IReadOnlyList<HomeDirectionCardViewModel> ParseDirections(string? raw) =>
        ParsePipeRows(raw, 5)
            .Select(parts =>
            {
                var isWide = parts.Length > 5 && string.Equals(parts[^1], "wide", StringComparison.OrdinalIgnoreCase);
                var trailingCount = isWide ? 3 : 2; // Url, LinkLabel, [wide]
                var urlIndex = parts.Length - trailingCount;

                return new HomeDirectionCardViewModel
                {
                    Number = parts[0],
                    Title = parts[1],
                    Description = string.Join(" | ", parts.Skip(2).Take(urlIndex - 2)),
                    Url = parts[urlIndex],
                    LinkLabel = parts[urlIndex + 1],
                    IsWide = isWide
                };
            })
            .ToList();

    // Title(0) | Description(1..) | Url | CssClassName
    // Same fold-back rule as ParseDirections above, applied to Description.
    public static IReadOnlyList<HomeFeatureCardViewModel> ParseFeatureCards(string? raw) =>
        ParsePipeRows(raw, 4)
            .Select(parts =>
            {
                var urlIndex = parts.Length - 2;

                return new HomeFeatureCardViewModel
                {
                    Title = parts[0],
                    Description = string.Join(" | ", parts.Skip(1).Take(urlIndex - 1)),
                    Url = parts[urlIndex],
                    CssClassName = parts[urlIndex + 1]
                };
            })
            .ToList();

    public static string JoinCoverageAreas(IEnumerable<HomeCoverageAreaItemViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x => $"{x.Icon} | {x.Title} | {x.Description}"));

    public static string JoinDirections(IEnumerable<HomeDirectionCardViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x =>
            $"{x.Number} | {x.Title} | {x.Description} | {x.Url} | {x.LinkLabel} | {(x.IsWide ? "wide" : string.Empty)}"));

    public static string JoinFeatureCards(IEnumerable<HomeFeatureCardViewModel> items) =>
        string.Join(Environment.NewLine, items.Select(x =>
            $"{x.Title} | {x.Description} | {x.Url} | {x.CssClassName}"));

    private static IReadOnlyList<string[]> ParsePipeRows(string? raw, int minSegments)
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
}
