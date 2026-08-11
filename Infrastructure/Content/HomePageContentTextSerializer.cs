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

    public static IReadOnlyList<HomeDirectionCardViewModel> ParseDirections(string? raw) =>
        ParsePipeRows(raw, 5)
            .Select(parts => new HomeDirectionCardViewModel
            {
                Number = parts[0],
                Title = parts[1],
                Description = parts[2],
                Url = parts[3],
                LinkLabel = parts[4],
                IsWide = parts.Length > 5 && string.Equals(parts[5], "wide", StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

    public static IReadOnlyList<HomeFeatureCardViewModel> ParseFeatureCards(string? raw) =>
        ParsePipeRows(raw, 4)
            .Select(parts => new HomeFeatureCardViewModel
            {
                Title = parts[0],
                Description = parts[1],
                Url = parts[2],
                CssClassName = parts[3]
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
